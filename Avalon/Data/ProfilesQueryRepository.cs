using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class ProfilesQueryRepository : IProfilesQueryRepository
    {
        private readonly ProfileContext _context = null;

        public ProfilesQueryRepository(IOptions<Settings> settings)
        {
            _context = new ProfileContext(settings);
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
                                .Set(p => p.UpdatedOn, DateTime.Now);

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
                                .Set(p => p.UpdatedOn, DateTime.Now);

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

        /////// <summary>Deletes the profiles.</summary>
        /////// <param name="profileIds">The profile identifiers.</param>
        /////// <returns></returns>
        ////public async Task<DeleteResult> DeleteProfiles(string[] profileIds)
        ////{
        ////    try
        ////    {
        ////        //return await _context.Profiles.DeleteManyAsync(
        ////        //    Builders<Profile>.Filter.Eq("ProfileId", profileIds));

        ////        return null;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        throw ex;
        ////    }
        ////}

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
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets curretUser's chatmember profiles.</summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetChatMemberProfiles(CurrentUser currentUser, int skip, int limit)
        {
            try
            {
                var chatMembers = currentUser.ChatMemberslist.Select(m => m.ProfileId);

                return await _context.Profiles.Find(p => chatMembers.Contains(p.ProfileId)).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

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
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the profile by name.</summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByName(string profileName)
        {
            try
            {
                var filter = Builders<Profile>
                                .Filter.Regex(p => p.Name, new BsonRegularExpression(profileName, "i")); 

                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        // Search for anything in filter - eg. { Body: 'something' }
        /// <summary>Gets the profile by filter.</summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfileByFilter(CurrentUser currentUser, ProfileFilter profileFilter, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

                //Remove currentUser from the list.
                filters.Add(Builders<Profile>.Filter.Ne(p => p.ProfileId, currentUser.ProfileId));

                //Add basic search criteria.
                filters.Add(Builders<Profile>.Filter.Eq(p => p.SexualOrientation, currentUser.SexualOrientation));

                if (currentUser.SexualOrientation == SexualOrientationType.Heterosexual || currentUser.SexualOrientation == SexualOrientationType.Homosexual) {
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.Gender, currentUser.ProfileFilter.Gender));
                }
                else
                {
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.Gender, profileFilter.Gender));
                }

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
                                .Find(combineFilters).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        private List<FilterDefinition<Profile>> ApplyProfileFilter(ProfileFilter profileFilter, List<FilterDefinition<Profile>> filters)
        {
            try
            {
                if (profileFilter.Name != null)
                    filters.Add(Builders<Profile>.Filter.Regex(p => p.Name, new BsonRegularExpression(profileFilter.Name, "i")));

                if (profileFilter.Age != null && profileFilter.Age[0] > 0)
                    filters.Add(Builders<Profile>.Filter.Gte(p => p.Age, profileFilter.Age[0]));

                if (profileFilter.Age != null && profileFilter.Age[1] > 0)
                    filters.Add(Builders<Profile>.Filter.Lte(p => p.Age, profileFilter.Age[1]));

                if (profileFilter.Height != null && profileFilter.Height[0] > 0)
                    filters.Add(Builders<Profile>.Filter.Gte(p => p.Height, profileFilter.Height[0]));

                if (profileFilter.Height != null && profileFilter.Height[1] > 0)
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
        /// <param name="sortDirection">The sort direction.</param>
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
                filters.Add(Builders<Profile>.Filter.Eq(p => p.SexualOrientation, currentUser.SexualOrientation));

                if (currentUser.SexualOrientation == SexualOrientationType.Heterosexual)
                    filters.Add(Builders<Profile>.Filter.Ne(p => p.Gender, currentUser.Gender));

                if (currentUser.SexualOrientation == SexualOrientationType.Homosexual)
                    filters.Add(Builders<Profile>.Filter.Eq(p => p.Gender, currentUser.Gender));

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
                            .Find(combineFilters).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the bookmarked profiles.</summary>
        /// <param name="currentUser">The current user.</param>
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
                                    .Find(p => currentUser.Bookmarks.Contains(p.ProfileId)).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
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
                
                foreach (var profile in profiles)
                {
                    if (profile.IsBookmarked.ContainsKey(currentUser.ProfileId))
                    {
                        profile.IsBookmarked[currentUser.ProfileId] = DateTime.Now;
                    }
                    else
                    {
                        var isBookmarkedPair = from pair in profile.IsBookmarked
                                               orderby pair.Value descending
                                               select pair;

                        if (isBookmarkedPair.Count() == 10)
                        {
                            profile.IsBookmarked.Remove(isBookmarkedPair.Last().Key);
                        }

                        profile.IsBookmarked.Add(currentUser.ProfileId, DateTime.Now);
                    }

                    var filter = Builders<Profile>
                               .Filter.Eq(p => p.ProfileId, profile.ProfileId);

                    var update = Builders<Profile>
                                .Update.Set(p => p.IsBookmarked, profile.IsBookmarked);

                    await _context.Profiles.FindOneAndUpdateAsync(filter, update);
                }
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

                foreach (var profile in profiles)
                {
                    if (profile.IsBookmarked.ContainsKey(currentUser.ProfileId))
                    {
                        profile.IsBookmarked.Remove(currentUser.ProfileId);
                    }

                    var filter = Builders<Profile>
                               .Filter.Eq(p => p.ProfileId, profile.ProfileId);

                    var update = Builders<Profile>
                                .Update.Set(p => p.IsBookmarked, profile.IsBookmarked);

                    await _context.Profiles.FindOneAndUpdateAsync(filter, update);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Add currentUser.profileId to visited list of profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profile"></param>
        public async Task AddVisitedToProfiles(CurrentUser currentUser, Profile profile)
        {
            try
            {
                if (profile.Visited.ContainsKey(currentUser.ProfileId))
                {
                    profile.Visited[currentUser.ProfileId] = DateTime.Now;
                }
                else
                {
                    var visitedPair = from pair in profile.Visited
                                      orderby pair.Value descending
                                      select pair;

                    if (visitedPair.Count() == 10)
                    {
                        profile.Visited.Remove(visitedPair.Last().Key);
                    }

                    profile.Visited.Add(currentUser.ProfileId, DateTime.Now);
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

        public async Task AddLikeToProfile(CurrentUser currentUser, Profile profile)
        {
            try
            {
                profile.Likes.Add(currentUser.ProfileId, DateTime.Now);

                var filter = Builders<Profile>
                           .Filter.Eq(p => p.ProfileId, profile.ProfileId);

                var update = Builders<Profile>
                            .Update.Set(p => p.Likes, profile.Likes);

                await _context.Profiles.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        public async Task RemoveLikeFromProfile(CurrentUser currentUser, Profile profile)
        {
            try
            {
                if (profile.Likes.ContainsKey(currentUser.ProfileId))
                {
                    profile.Likes.Remove(currentUser.ProfileId);
                }

                var filter = Builders<Profile>
                           .Filter.Eq(p => p.ProfileId, profile.ProfileId);

                var update = Builders<Profile>
                            .Update.Set(p => p.Likes, profile.Likes);

                await _context.Profiles.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Maintenance

        /// <summary>Gets 10 old profiles that are more than 30 days since last active.</summary>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetOldProfiles()
        {
            try
            {
                return await _context.Profiles.Find(p => p.LastActive < DateTime.Now.AddDays(-30) && !p.Admin).Limit(10).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
