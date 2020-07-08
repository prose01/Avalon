using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
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
                                .Filter.Eq(e => e.ProfileId, profileId);

                var update = Builders<Profile>
                                .Update.Set(e => e.Admin, true)
                                .Set(e => e.UpdatedOn, DateTime.Now);

                var options = new FindOneAndUpdateOptions<Profile>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return await _context.Profiles.FindOneAndUpdateAsync(filter, update, options);
            }
            catch (Exception ex)
            {
                throw ex;
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
                                .Filter.Eq(e => e.ProfileId, profileId);

                var update = Builders<Profile>
                                .Update.Set(e => e.Admin, false)
                                .Set(e => e.UpdatedOn, DateTime.Now);

                var options = new FindOneAndUpdateOptions<Profile>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return await _context.Profiles.FindOneAndUpdateAsync(filter, update, options);
            }
            catch (Exception ex)
            {
                throw ex;
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
            catch (Exception ex)
            {
                throw ex;
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
            var filter = Builders<Profile>
                            .Filter.Eq(e => e.ProfileId, profileId);

            try
            {
                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets curretUser's chatmember profiles.</summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetChatMemberProfiles(CurrentUser currentUser)
        {
            try
            {
                var chatMembers = currentUser.ChatMemberslist.Select(m => m.ProfileId);

                return await _context.Profiles.Find(p => chatMembers.Contains(p.ProfileId)).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets the profile by Auth0Id.</summary>
        /// <param name="auth0Id">Auth0Id of the profile.</param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByAuth0Id(string auth0Id)
        {
            var filter = Builders<Profile>
                            .Filter.Eq(e => e.Auth0Id, auth0Id);

            try
            {
                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets the profile by name.</summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByName(string profileName)
        {
            var filter = Builders<Profile>
                            .Filter.Eq(e => e.Name, profileName); // TODO: Make Name case insensitivity

            try
            {
                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Search for anything in filter - eg. { Body: 'something' }
        /// <summary>Gets the profile by filter.</summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetProfileByFilter(CurrentUser currentUser, ProfileFilter profileFilter)
        {
            List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

            //Remove currentUser from the list.
            filters.Add(Builders<Profile>.Filter.Ne(x => x.ProfileId, currentUser.ProfileId));
            filters.Add(Builders<Profile>.Filter.Eq(x => x.SexualOrientation, currentUser.SexualOrientation));

            if (currentUser.SexualOrientation == SexualOrientationType.Heterosexual)
                filters.Add(Builders<Profile>.Filter.Ne(x => x.Gender, currentUser.Gender));

            if (currentUser.SexualOrientation == SexualOrientationType.Homosexual)
                filters.Add(Builders<Profile>.Filter.Eq(x => x.Gender, currentUser.Gender));

            if (!string.IsNullOrEmpty(profileFilter.Name))
                filters.Add(Builders<Profile>.Filter.Eq(x => x.Name, profileFilter.Name));  // TODO: Make Name case insensitivity

            if (profileFilter.Age != null)
                filters.Add(Builders<Profile>.Filter.In(x => x.Age, profileFilter.Age));

            var combineFilters = Builders<Profile>.Filter.And(filters);

            try
            {
                return _context.Profiles
                    .Find(combineFilters).ToList().OrderByDescending(p => p.CreatedOn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets the latest created profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetLatestCreatedProfiles(CurrentUser currentUser)
        {
            List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

            //Remove currentUser from the list.
            filters.Add(Builders<Profile>.Filter.Ne(x => x.ProfileId, currentUser.ProfileId));
            filters.Add(Builders<Profile>.Filter.Eq(x => x.SexualOrientation, currentUser.SexualOrientation));

            if (currentUser.SexualOrientation == SexualOrientationType.Heterosexual)
                filters.Add(Builders<Profile>.Filter.Ne(x => x.Gender, currentUser.Gender));

            if (currentUser.SexualOrientation == SexualOrientationType.Homosexual)
                filters.Add(Builders<Profile>.Filter.Eq(x => x.Gender, currentUser.Gender));

            var combineFilters = Builders<Profile>.Filter.And(filters);

            try
            {
                return _context.Profiles
                    .Find(combineFilters).ToList().OrderByDescending(p => p.CreatedOn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Bookmarked 
        // Bør nok reduceres til kun GetBookmarkedProfiles da filtreringen kan ske i frontend. 

        /// <summary>Gets the bookmarked profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles(CurrentUser currentUser)
        {
            try
            {
                // Get all other Profiles from ProfileIds
                var query = _context.Profiles.Find(p => currentUser.Bookmarks.Contains(p.ProfileId));  // Check for null/ingen bookmarks

                return await Task.FromResult(query.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
