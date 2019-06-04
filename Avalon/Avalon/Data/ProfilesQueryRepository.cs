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

        public async Task<IEnumerable<Profile>> GetLatestProfiles(Profile currentUser)
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
                _logger.LogWarning(ex, "GetLatestProfiles threw an exception.");
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

        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles(Profile currentUser, string profileId)
        {
            try
            {
                // Get all Bookmarked ProfileIds from original profile.
                var bookmarks = _context.Profiles.AsQueryable()
                    .Where(p => p.ProfileId == profileId && p.Email != currentUser.Email)
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
    }
}
