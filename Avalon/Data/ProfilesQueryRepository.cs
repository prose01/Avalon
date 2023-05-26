using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace Avalon.Data
{
    public class ProfilesQueryRepository : IProfilesQueryRepository
    {
        private readonly Context _context = null;
        private readonly long _minAge;
        private readonly long _maxAge;
        private readonly long _minHeight;
        private readonly long _maxHeight;
        private readonly long _maxIsBookmarked;
        private readonly long _maxVisited;
        private int _deleteProfileDaysBack;
        private int _deleteProfileLimit;
        private int _complainsDaysBack;
        private int _complainsWarnUser;
        private int _complainsDeleteUser;

        public ProfilesQueryRepository(IOptions<Settings> settings, IConfiguration config)
        {
            _context = new Context(settings);
            _minAge = config.GetValue<long>("MinAge");
            _maxAge = config.GetValue<long>("MaxAge");
            _minHeight = config.GetValue<long>("MinHeight");
            _maxHeight = config.GetValue<long>("MaxHeight");
            _maxIsBookmarked = config.GetValue<long>("MaxIsBookmarked");
            _maxVisited = config.GetValue<long>("MaxVisited");
            _deleteProfileDaysBack = config.GetValue<int>("DeleteProfileDaysBack");
            _deleteProfileLimit = config.GetValue<int>("DeleteProfileLimit");
            _complainsDaysBack = config.GetValue<int>("ComplainsDaysBack");
            _complainsWarnUser = config.GetValue<int>("ComplainsWarnUser");
            _complainsDeleteUser = config.GetValue<int>("ComplainsDeleteUser");
        }

        #region Admin stuff

        /// <summary>Set profile as admin.</summary>
        /// <param name="item">The profile.</param>
        /// <returns></returns>
        public async Task<Profile> SetAsAdmin(string profileId)
        {
            try
            {
                var filter = Builders<Profile>
                                .Filter.Eq(p => p.ProfileId, profileId);

                var update = Builders<Profile>
                                .Update.Set(p => p.Admin, true)
                                .Set(p => p.UpdatedOn, DateTime.UtcNow);

                var options = new FindOneAndUpdateOptions<Profile>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return await _context.Profiles.FindOneAndUpdateAsync(filter, update, options);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Removes profile as admin.</summary>
        /// <param name="item">The profile.</param>
        /// <returns></returns>
        public async Task<Profile> RemoveAdmin(string profileId)
        {
            try
            {
                var filter = Builders<Profile>
                                .Filter.Eq(p => p.ProfileId, profileId);

                var update = Builders<Profile>
                                .Update.Set(p => p.Admin, false)
                                .Set(p => p.UpdatedOn, DateTime.UtcNow);

                var options = new FindOneAndUpdateOptions<Profile>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return await _context.Profiles.FindOneAndUpdateAsync(filter, update, options);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Profiles

        /// <summary>Delete the profile. There is no going back!</summary>
        /// <param name="profileIds">The profile identifier.</param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteProfile(string profileId)
        {
            try
            {
                return await _context.Profiles.DeleteOneAsync(
                    Builders<Profile>.Filter.Eq("ProfileId", profileId));
            }
            catch
            {
                throw;
            }
        }

        ///// <summary>Delete list of profiles. There is no going back!</summary>
        ///// <param name="profileIds">The profile identifiers.</param>
        ///// <returns></returns>
        //public async Task<DeleteResult> DeleteProfiles(string[] profileIds)     // TODO: Cannot be used until we have a delete many version of DeleteProfileFromAuth0!
        //{
        //    try
        //    {
        //        return await _context.Profiles.DeleteManyAsync(
        //            Builders<Profile>.Filter.Eq("ProfileId", profileIds));
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        /// <summary>Gets the profile by identifier.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        public async Task<Profile> GetProfileById(string profileId)
        {
            try
            {
                var filter = Builders<Profile>
                                .Filter.Eq(p => p.ProfileId, profileId);

                return await _context.Profiles
                    .Find(filter)
                    .Project<Profile>(this.GetProjection())
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets profiles by identifiers.</summary>
        /// <param name="profileId">The profile identifiers.</param>        // TODO: This can be moved to another application when CleanCurrentUser is moved!!!
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfilesByIds(string[] profileIds)
        {
            try
            {
                var filter = Builders<Profile>
                                .Filter.In(p => p.ProfileId, profileIds);

                return await _context.Profiles
                    .Find(filter)
                    .Project<Profile>(GetProfileId())
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets profiles by identifiers.</summary>
        /// <param name="profileId">The profile identifiers.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfilesByIds(CurrentUser currentUser, string[] profileIds, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                //Remove currentUser from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.ProfileId, currentUser.ProfileId));

                //Remove admins from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.Admin, true));

                filters.Add(Builders<Profile>.Filter.Eq(p => p.Countrycode, currentUser.Countrycode));

                filters.Add(Builders<Profile>.Filter.In(p => p.ProfileId, profileIds));

                var combineFilters = Builders<Profile>.Filter.And(filters);

                //var filter = Builders<Profile>
                //                .Filter.In(p => p.ProfileId, profileIds);

                SortDefinition<Profile> sortDefinition = Builders<Profile>.Sort.Descending(p => p.Name);

                return await _context.Profiles
                            .Find(combineFilters).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets curretUser's chatmember profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        //public async Task<IEnumerable<Profile>> GetChatMemberProfiles(CurrentUser currentUser, int skip, int limit)
        //{
        //    try
        //    {
        //        var chatMembers = currentUser.ChatMemberslist.Select(m => m.ProfileId);

        //        return await _context.Profiles.Find(p => chatMembers.Contains(p.ProfileId)).Project<Profile>(this.GetProjection()).Skip(skip).Limit(limit).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        /// <summary>Gets the profile by Auth0Id.</summary>
        /// <param name="auth0Id">Auth0Id of the profile.</param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByAuth0Id(string auth0Id)
        {
            try
            {
                var filter = Builders<Profile>
                                .Filter.Eq(p => p.Auth0Id, auth0Id);
                return await _context.Profiles
                    .Find(filter)
                    .Project<Profile>(this.GetProjection())
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        // Search for anything in filter - eg. { Body: 'something' }
        /// <summary>Gets the profile by filter.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileFilter">The profileFilter.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfileByFilter(CurrentUser currentUser, ProfileFilter profileFilter, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                //Remove currentUser from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.ProfileId, currentUser.ProfileId));

                //Remove admins from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.Admin, true));

                filters.Add(Builders<Profile>.Filter.Eq(p => p.Countrycode, currentUser.Countrycode));

                filters.Add(Builders<Profile>.Filter.In(p => p.Gender, currentUser.Seeking));

                filters.Add(Builders<Profile>.Filter.Where(p => p.Seeking.Contains(currentUser.Gender)));

                //Apply all other ProfileFilter criterias.
                filters = this.ApplyProfileFilter(profileFilter, filters);

                var combineFilters = Builders<Profile>.Filter.And(filters);

                SortDefinition<Profile> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.UpdatedOn:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.UpdatedOn);
                        break;
                    case OrderByType.LastActive:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.LastActive);
                        break;
                    default:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.CreatedOn);
                        break;
                }

                return await _context.Profiles
                                .Find(combineFilters).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Applies definitions to filter.</summary>
        /// <param name="profileFilter">The profileFilter.</param>
        /// <param name="filters">The filterDefinition.</param>
        private List<FilterDefinition<Profile>> ApplyProfileFilter(ProfileFilter profileFilter, List<FilterDefinition<Profile>> filters)
        {
            try
            {
                if (profileFilter.Name != null)
                    filters.Add(Builders<Profile>.Filter.Regex(p => p.Name, new BsonRegularExpression(profileFilter.Name, "i")));

                if (profileFilter.Age != null && profileFilter.Age[0] > this._minAge)
                    filters.Add(Builders<Profile>.Filter.Gte(p => p.Age, profileFilter.Age[0]));

                if (profileFilter.Age != null && profileFilter.Age[1] < this._maxAge)
                    filters.Add(Builders<Profile>.Filter.Lte(p => p.Age, profileFilter.Age[1]));

                if (profileFilter.Height != null && profileFilter.Height[0] > this._minHeight)
                    filters.Add(Builders<Profile>.Filter.Gte(p => p.Height, profileFilter.Height[0]));

                if (profileFilter.Height != null && profileFilter.Height[1] < this._maxHeight)
                    filters.Add(Builders<Profile>.Filter.Lte(p => p.Height, profileFilter.Height[1]));

                if (profileFilter.Description != null)
                    filters.Add(Builders<Profile>.Filter.Regex(p => p.Description, new BsonRegularExpression(profileFilter.Description, "i")));

                if (profileFilter.Tags != null && profileFilter.Tags.Count > 0)
                    filters.Add(Builders<Profile>.Filter.AnyIn(p => p.Tags, profileFilter.Tags));

                if (profileFilter.Body != BodyType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.Body, profileFilter.Body));

                if (profileFilter.SmokingHabits != SmokingHabitsType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.SmokingHabits, profileFilter.SmokingHabits));

                if (profileFilter.HasChildren != HasChildrenType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.HasChildren, profileFilter.HasChildren));

                if (profileFilter.WantChildren != WantChildrenType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.WantChildren, profileFilter.WantChildren));

                if (profileFilter.HasPets != HasPetsType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.HasPets, profileFilter.HasPets));

                if (profileFilter.LivesIn != LivesInType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.LivesIn, profileFilter.LivesIn));

                if (profileFilter.Education != EducationType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.Education, profileFilter.Education));

                if (profileFilter.EducationStatus != EducationStatusType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.EducationStatus, profileFilter.EducationStatus));

                if (profileFilter.EmploymentStatus != EmploymentStatusType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.EmploymentStatus, profileFilter.EmploymentStatus));

                if (profileFilter.SportsActivity != SportsActivityType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.SportsActivity, profileFilter.SportsActivity));

                if (profileFilter.EatingHabits != EatingHabitsType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.EatingHabits, profileFilter.EatingHabits));

                if (profileFilter.ClotheStyle != ClotheStyleType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.ClotheStyle, profileFilter.ClotheStyle));

                if (profileFilter.BodyArt != BodyArtType.NotChosen)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.BodyArt, profileFilter.BodyArt));

                return filters;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the latest profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetLatestProfiles(CurrentUser currentUser, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                //Remove currentUser from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.ProfileId, currentUser.ProfileId));

                //Remove admins from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.Admin, true));

                filters.Add(Builders<Profile>.Filter.Eq(p => p.Countrycode, currentUser.Countrycode));

                filters.Add(Builders<Profile>.Filter.In(p => p.Gender, currentUser.Seeking));

                filters.Add(Builders<Profile>.Filter.Where(p => p.Seeking.Contains(currentUser.Gender)));

                var combineFilters = Builders<Profile>.Filter.And(filters);

                SortDefinition<Profile> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.UpdatedOn:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.UpdatedOn);
                        break;
                    case OrderByType.LastActive:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.LastActive);
                        break;
                    default:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.CreatedOn);
                        break;
                }

                return await _context.Profiles
                            .Find(combineFilters).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the bookmarked profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles(CurrentUser currentUser, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                SortDefinition<Profile> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.UpdatedOn:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.UpdatedOn);
                        break;
                    case OrderByType.LastActive:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.LastActive);
                        break;
                    default:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.CreatedOn);
                        break;
                }

                return await _context.Profiles
                            .Find(p => currentUser.Bookmarks.Contains(p.ProfileId)).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets Profiles who has visited my profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfilesWhoVisitedMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                filters.Add(Builders<Profile>.Filter.In(p => p.ProfileId, currentUser.Visited.Keys));

                //Remove admins from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.Admin, true));

                var combineFilters = Builders<Profile>.Filter.And(filters);

                SortDefinition<Profile> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.UpdatedOn:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.UpdatedOn);
                        break;
                    case OrderByType.LastActive:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.LastActive);
                        break;
                    default:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.CreatedOn);
                        break;
                }

                return await _context.Profiles
                            .Find(combineFilters).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets Profiles who has bookmarked my profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfilesWhoBookmarkedMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                filters.Add(Builders<Profile>.Filter.In(p => p.ProfileId, currentUser.IsBookmarked.Keys));

                //Remove admins from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.Admin, true));

                var combineFilters = Builders<Profile>.Filter.And(filters);

                SortDefinition<Profile> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.UpdatedOn:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.UpdatedOn);
                        break;
                    case OrderByType.LastActive:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.LastActive);
                        break;
                    default:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.CreatedOn);
                        break;
                }

                return await _context.Profiles
                            .Find(combineFilters).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets Profiles who likes my profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfilesWhoLikesMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                filters.Add(Builders<Profile>.Filter.In(p => p.ProfileId, currentUser.Likes));

                //Remove admins from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.Admin, true));

                var combineFilters = Builders<Profile>.Filter.And(filters);

                SortDefinition<Profile> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.UpdatedOn:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.UpdatedOn);
                        break;
                    case OrderByType.LastActive:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.LastActive);
                        break;
                    default:
                        sortDefinition = Builders<Profile>.Sort.Descending(p => p.CreatedOn);
                        break;
                }

                return await _context.Profiles
                            .Find(combineFilters).Project<Profile>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Add currentUser.profileId to IsBookmarked list of every profile in profileIds list.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        public async Task AddIsBookmarkedToProfiles(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                var query = _context.Profiles.Find(p => profileIds.Contains(p.ProfileId));

                var profiles = await Task.FromResult(query.ToList());

                var update = Builders<Profile>.Update;
                var updates = new List<UpdateDefinition<Profile>>();

                foreach (var profile in profiles)
                {
                    var isBookmarkedPair = from pair in profile.IsBookmarked
                                           orderby pair.Value descending
                                           select pair;

                    if (isBookmarkedPair.Count() > _maxIsBookmarked)
                    {
                        profile.IsBookmarked.Remove(isBookmarkedPair.Last().Key);
                    }

                    profile.IsBookmarked.Add(currentUser.ProfileId, DateTime.UtcNow);

                    updates.Add(update.Set(p => p.IsBookmarked, profile.IsBookmarked));
                }

                var filter = Builders<Profile>
                                .Filter.In(p => p.ProfileId, profileIds);

                await _context.Profiles.UpdateManyAsync(filter, update.Combine(updates));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Remove currentUser.profileId from IsBookmarked list of every profile in profileIds list.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        public async Task RemoveIsBookmarkedFromProfiles(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                var query = _context.Profiles.Find(p => profileIds.Contains(p.ProfileId));

                var profiles = await Task.FromResult(query.ToList());

                var update = Builders<Profile>.Update;
                var updates = new List<UpdateDefinition<Profile>>();

                foreach (var profile in profiles)
                {
                    if (profile.IsBookmarked.ContainsKey(currentUser.ProfileId))
                    {
                        profile.IsBookmarked.Remove(currentUser.ProfileId);

                        updates.Add(update.Set(p => p.IsBookmarked, profile.IsBookmarked));
                    }
                }

                var filter = Builders<Profile>
                                .Filter.In(p => p.ProfileId, profileIds);

                if (updates.Count == 0)
                    return;

                await _context.Profiles.UpdateManyAsync(filter, update.Combine(updates));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Add currentUser.profileId to visited list of profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profile">The profile.</param>
        public async Task AddVisitedToProfiles(CurrentUser currentUser, Profile profile)
        {
            try
            {
                if (profile.Visited.ContainsKey(currentUser.ProfileId))
                {
                    profile.Visited[currentUser.ProfileId] = DateTime.UtcNow;
                }
                else
                {
                    var visitedPair = from pair in profile.Visited
                                      orderby pair.Value descending
                                      select pair;

                    if (visitedPair.Count() > _maxVisited)
                    {
                        profile.Visited.Remove(visitedPair.Last().Key);
                    }

                    profile.Visited.Add(currentUser.ProfileId, DateTime.UtcNow);
                }

                var filter = Builders<Profile>
                           .Filter.Eq(p => p.ProfileId, profile.ProfileId);

                var update = Builders<Profile>
                            .Update.Set(p => p.Visited, profile.Visited);

                await _context.Profiles.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Add currentUser.profileId to likes list of profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>

        public async Task AddLikeToProfiles(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                var query = _context.Profiles.Find(p => profileIds.Contains(p.ProfileId));

                var profiles = await Task.FromResult(query.ToList());

                var update = Builders<Profile>.Update;
                var updates = new List<UpdateDefinition<Profile>>();

                foreach (var profile in profiles)
                {
                    // If already added just leave.
                    if (profile.Likes.Contains(currentUser.ProfileId))
                        continue;

                    profile.Likes.Add(currentUser.ProfileId);

                    updates.Add(update.Set(p => p.Likes, profile.Likes));
                }

                var filter = Builders<Profile>
                                .Filter.In(p => p.ProfileId, profileIds);

                await _context.Profiles.UpdateManyAsync(filter, update.Combine(updates));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Removes currentUser.profileId from likes list of profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>

        public async Task RemoveLikeFromProfiles(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                var query = _context.Profiles.Find(p => profileIds.Contains(p.ProfileId));

                var profiles = await Task.FromResult(query.ToList());

                var update = Builders<Profile>.Update;
                var updates = new List<UpdateDefinition<Profile>>();

                foreach (var profile in profiles)
                {
                    if (profile.Likes.Contains(currentUser.ProfileId))
                    {
                        profile.Likes.Remove(currentUser.ProfileId);

                        updates.Add(update.Set(p => p.Likes, profile.Likes));
                    }
                }

                var filter = Builders<Profile>
                                .Filter.In(p => p.ProfileId, profileIds);

                await _context.Profiles.UpdateManyAsync(filter, update.Combine(updates));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Add currentUser.profileId to complains list of profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile.</param>
        public async Task AddComplainToProfile(CurrentUser currentUser, Profile profile)
        {
            try
            {
                // Remove old complains.
                var oldcomplains = profile.Complains.Where(i => i.Value < DateTime.UtcNow.AddDays(-_complainsDaysBack)).ToArray();

                if (oldcomplains.Length > 0)
                {
                    foreach (var oldcomplain in oldcomplains)
                    {
                        profile.Complains.Remove(oldcomplain.Key.ToString());
                    }
                }

                // Add new or update complain
                if (profile.Complains.ContainsKey(currentUser.ProfileId))
                {
                    profile.Complains[currentUser.ProfileId] = DateTime.UtcNow;
                }
                else
                {
                    var complainPair = from pair in profile.Complains
                                       orderby pair.Value descending
                                       select pair;


                    profile.Complains.Add(currentUser.ProfileId, DateTime.UtcNow);

                    // Delete profile if too many complains
                    if (profile.Complains.Count >= _complainsDeleteUser)
                    {
                        //await this.DeleteProfile(profile.ProfileId);
                        return;
                    }

                    // Send warning to profile if too many complains
                    if (profile.Complains.Count >= _complainsWarnUser)
                    {
                        //warning; // TODO: Send warning to profile if too many complains
                    }
                }

                var filter = Builders<Profile>
                               .Filter.Eq(p => p.ProfileId, profile.ProfileId);

                var update = Builders<Profile>
                            .Update.Set(p => p.Complains, profile.Complains);

                await _context.Profiles.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        private ProjectionDefinition<Profile> GetProjection()
        {
            ProjectionDefinition<Profile> projection = "{ " +
                "_id: 0, " +
                "Auth0Id: 0, " +
                "Admin:0, " +
                "Gender: 0, " +
                "Seeking: 0, " +
                "Bookmarks: 0, " +
                "ChatMemberslist: 0, " +
                "ProfileFilter: 0, " +
                "IsBookmarked: 0, " +
                "Languagecode: 0, " +
                "Groups: 0, " +
                "}";

            return projection;
        }

        private ProjectionDefinition<Profile> GetProfileId()
        {
            ProjectionDefinition<Profile> projection = "{ " +
                "_id: 0, " +
                "Auth0Id: 0, " +
                "Admin: 0, " +
                "Name: 0, " +
                "CreatedOn: 0, " +
                "UpdatedOn: 0, " +
                "LastActive: 0, " +
                "Age: 0, " +
                "Height: 0, " +
                "Contactable: 0, " +
                "Description: 0, " +
                "Images: 0, " +
                "Tags: 0, " +
                "Body: 0, " +
                "SmokingHabits: 0, " +
                "HasChildren: 0, " +
                "WantChildren: 0, " +
                "HasPets: 0, " +
                "LivesIn: 0, " +
                "Education: 0, " +
                "EducationStatus: 0, " +
                "EmploymentStatus: 0, " +
                "SportsActivity: 0, " +
                "EatingHabits: 0, " +
                "ClotheStyle: 0, " +
                "BodyArt: 0, " +
                "Gender: 0, " +
                "Seeking: 0, " +
                "Bookmarks: 0, " +
                "ChatMemberslist: 0, " +
                "ProfileFilter: 0, " +
                "IsBookmarked: 0, " +
                "Languagecode: 0, " +
                "Visited: 0, " +
                "Likes: 0, " +
                "Groups: 0, " +
                "}";

            return projection;
        }

        #endregion

        #region Maintenance

        /// <summary>Gets 10 old profiles (limit) that are more than 30 days (daysBack) since last active.</summary>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetOldProfiles(int daysBack, int limit)
        {
            try
            {
                _deleteProfileDaysBack = daysBack > 0 ? daysBack : _deleteProfileDaysBack;
                _deleteProfileLimit = limit > 0 ? limit : _deleteProfileLimit;

                return await _context.Profiles.Find(p => p.LastActive < DateTime.UtcNow.AddDays(-_deleteProfileDaysBack) && !p.Admin).Project<Profile>(this.GetProjection()).Limit(_deleteProfileLimit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
