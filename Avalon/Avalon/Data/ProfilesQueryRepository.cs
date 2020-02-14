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

        #region Profiles

        /// <summary>Gets all profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetAllProfiles(CurrentUser currentUser)
        {
            try
            {
                return await _context.Profiles.Find(p => true && p.Email != currentUser.Email).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

        /// <summary>Gets the profile by name.</summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByName(string profileName)
        {
            var filter = Builders<Profile>
                            .Filter.Eq(e => e.Name, profileName);

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
        public async Task<IEnumerable<Profile>> GetProfileByFilter(ProfileFilter profileFilter)
        {
            List<FilterDefinition<Profile>> filters = new List<FilterDefinition<Profile>>();

            //Remove currentUser from the list.
            filters.Add(Builders<Profile>.Filter.Ne(x => x.ProfileId, profileFilter.CurrentUserId));

            if (profileFilter.Name != null)
                filters.Add(Builders<Profile>.Filter.Eq(x => x.Name, profileFilter.Name));

            if (profileFilter.Age != null)
                filters.Add(Builders<Profile>.Filter.In(x => x.Age, profileFilter.Age));

            var combineFilters = Builders<Profile>.Filter.And(filters);

            try
            {
                return _context.Profiles
                    .Find(combineFilters).ToList();
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
            try
            {
                var query = _context.Profiles.AsQueryable()
                    .Where(p => true && p.Email != currentUser.Email)
                    .OrderByDescending(p => p.CreatedOn).Take(2);

                return await Task.FromResult(query.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets the last updated profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetLastUpdatedProfiles(CurrentUser currentUser)
        {
            try
            {
                var query = _context.Profiles.AsQueryable()
                    .Where(p => true && p.Email != currentUser.Email)
                    .OrderByDescending(p => p.UpdatedOn).Take(2);

                return await Task.FromResult(query.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets the last active profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetLastActiveProfiles(CurrentUser currentUser)
        {
            try
            {
                var query = _context.Profiles.AsQueryable()
                    .Where(p => true && p.Email != currentUser.Email)
                    .OrderByDescending(p => p.LastActive).Take(2);

                return await Task.FromResult(query.ToList());
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
                //// Get all Bookmarked ProfileIds from original profile.
                //var bookmarks = _context.Profiles.AsQueryable()
                //    .Where(p => p.Email != currentUser.Email)
                //    .Select(p => new {p.Bookmarks});

                //var bookmarkedProfileIds = await Task.FromResult(bookmarks.ToList());

                // Get all other Profiles from ProfileIds
                var query = _context.Profiles.Find(p => currentUser.Bookmarks.Contains(p.ProfileId));

                return await Task.FromResult(query.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Bør nok reduceres til kun GetBookmarkedProfiles da filtreringen kan ske i frontend. 

        //public async Task<IEnumerable<AbstractProfile>> GetBookmarkedLatestCreatedProfiles(AbstractProfile currentUser)
        //{
        //    try
        //    {
        //        // Get all Bookmarked ProfileIds from original profile.
        //        var bookmarks = _context.Profiles.AsQueryable()
        //            .Where(p => p.Email == currentUser.Email)
        //            .Select(p => new { p.Bookmarks });

        //        var bookmarkedProfileIds = await Task.FromResult(bookmarks.ToList());

        //        // Get all other Profiles from ProfileIds ordered by create
        //        var query = _context.Profiles.AsQueryable()
        //            .Where(p => bookmarkedProfileIds.First().Bookmarks.Contains(p.ProfileId))
        //            .OrderByDescending(p => p.CreatedOn).Take(2);

        //        return await Task.FromResult(query.ToList());
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public async Task<IEnumerable<AbstractProfile>> GetBookmarkedLastUpdatedProfiles(AbstractProfile currentUser)
        //{
        //    try
        //    {
        //        // Get all Bookmarked ProfileIds from original profile.
        //        var bookmarks = _context.Profiles.AsQueryable()
        //            .Where(p => p.Email == currentUser.Email)
        //            .Select(p => new { p.Bookmarks });

        //        var bookmarkedProfileIds = await Task.FromResult(bookmarks.ToList());

        //        // Get all other Profiles from ProfileIds ordered by update
        //        var query = _context.Profiles.AsQueryable()
        //            .Where(p => bookmarkedProfileIds.First().Bookmarks.Contains(p.ProfileId))
        //            .OrderByDescending(p => p.UpdatedOn).Take(2);

        //        return await Task.FromResult(query.ToList());
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public async Task<IEnumerable<AbstractProfile>> GetBookmarkedLastActiveProfiles(AbstractProfile currentUser)
        //{
        //    try
        //    {
        //        // Get all Bookmarked ProfileIds from original profile.
        //        var bookmarks = _context.Profiles.AsQueryable()
        //            .Where(p => p.Email == currentUser.Email)
        //            .Select(p => new { p.Bookmarks });

        //        var bookmarkedProfileIds = await Task.FromResult(bookmarks.ToList());

        //        // Get all other Profiles from ProfileIds ordered by active
        //        var query = _context.Profiles.AsQueryable()
        //            .Where(p => bookmarkedProfileIds.First().Bookmarks.Contains(p.ProfileId))
        //            .OrderByDescending(p => p.LastActive).Take(2);

        //        return await Task.FromResult(query.ToList());
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion
    }
}
