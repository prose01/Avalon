using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProfilesQueryRepository> _logger;

        public ProfilesQueryRepository(IOptions<Settings> settings, ILogger<ProfilesQueryRepository> logger)
        {
            _context = new ProfileContext(settings);
            _logger = logger;
        }

        #region Profiles

        public async Task<IEnumerable<Profile>> GetLatestCreatedProfiles(Profile currentUser)
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
                // log or manage the exception
                _logger.LogWarning(ex, "GetLatestCreatedProfiles threw an exception.");
                throw ex;
            }
        }

        public async Task<IEnumerable<Profile>> GetLastUpdatedProfiles(Profile currentUser)
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
                // log or manage the exception
                _logger.LogWarning(ex, "GetLastUpdatedProfiles threw an exception.");
                throw ex;
            }
        }

        public async Task<IEnumerable<Profile>> GetLastActiveProfiles(Profile currentUser)
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
                // log or manage the exception
                _logger.LogWarning(ex, "GetLatestActiveProfiles threw an exception.");
                throw ex;
            }
        }

        #endregion

        #region Bookmarked 
        // Bør nok reduceres til kun GetBookmarkedProfiles da filtreringen kan ske i frontend. 

        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles(Profile currentUser)
        {
            try
            {
                // Get all Bookmarked ProfileIds from original profile.
                var bookmarks = _context.Profiles.AsQueryable()
                    .Where(p => p.Email != currentUser.Email)
                    .Select(p => new {p.Bookmarks});

                var bookmarkedProfileIds = await Task.FromResult(bookmarks.ToList());

                // Get all other Profiles from ProfileIds
                var query = _context.Profiles.Find(p => bookmarkedProfileIds.First().Bookmarks.Contains(p.ProfileId));

                return await Task.FromResult(query.ToList());
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "GetBookmarkedProfiles threw an exception.");
                throw ex;
            }
        }

        // Bør nok reduceres til kun GetBookmarkedProfiles da filtreringen kan ske i frontend. 

        //public async Task<IEnumerable<Profile>> GetBookmarkedLatestCreatedProfiles(Profile currentUser)
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
        //        // log or manage the exception
        //        _logger.LogWarning(ex, "GetBookmarkedLatestCreatedProfiles threw an exception.");
        //        throw ex;
        //    }
        //}

        //public async Task<IEnumerable<Profile>> GetBookmarkedLastUpdatedProfiles(Profile currentUser)
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
        //        // log or manage the exception
        //        _logger.LogWarning(ex, "GetBookmarkedLastUpdatedProfiles threw an exception.");
        //        throw ex;
        //    }
        //}

        //public async Task<IEnumerable<Profile>> GetBookmarkedLastActiveProfiles(Profile currentUser)
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
        //        // log or manage the exception
        //        _logger.LogWarning(ex, "GetBookmarkedLastActiveProfiles threw an exception.");
        //        throw ex;
        //    }
        //}

        #endregion
    }
}
