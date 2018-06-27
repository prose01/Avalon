using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ProfileContext _context = null;
        private readonly ILogger<ProfileRepository> _logger;

        public ProfileRepository(IOptions<Settings> settings, ILogger<ProfileRepository> logger)
        {
            _context = new ProfileContext(settings);
            _logger = logger;
        }

        public async Task<IEnumerable<Profile>> GetAllProfiles()
        {
            try
            {
                //return null;
                return await _context.Profiles.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "GetAllProfiles threw an exception.");
                throw ex;
            }
        }

        public async Task<Profile> GetProfile(string profileId)
        {
            var filter = Builders<Profile>.Filter.Eq("ProfileId", profileId);

            try
            {
                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "GetProfile threw an exception.");
                throw ex;
            }
        }

        public async Task<Profile> GetProfileByName(string profileName)
        {
            var filter = Builders<Profile>.Filter.Eq("Name", profileName);

            try
            {
                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "GetProfileByName threw an exception.");
                throw ex;
            }
        }

        public async Task AddProfile(Profile item)
        {
            try
            {
                // TODO: create Id
                item.ProfileId = Guid.NewGuid().ToString();
                item.CreatedOn = DateTime.Now;
                item.UpdatedOn = DateTime.Now;

                //await _context.Profiles.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "AddProfile threw an exception.");
                throw ex;
            }
        }

        public async Task<DeleteResult> RemoveProfile(string profileId)
        {
            try
            {
                return await _context.Profiles.DeleteOneAsync(
                    Builders<Profile>.Filter.Eq("ProfileId", profileId));
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "RemoveProfile threw an exception.");
                throw ex;
            }
        }

        private async Task<ReplaceOneResult> UpdateProfileDocument(string profileId, Profile item)
        {
            try
            {
                //return await _context.Profiles
                //            .ReplaceOneAsync(p => p.ProfileId.Equals(profileId)
                //                            , item
                //                            , new UpdateOptions { IsUpsert = true });

                return null;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "UpdateProfileDocument threw an exception.");
                throw ex;
            }
        }

        //Demo function - full document update
        public async Task<ReplaceOneResult> UpdateProfile(string profileId, Profile item)
        {
            try
            {
                item.UpdatedOn = DateTime.Now;

                //return await UpdateProfileDocument(profileId, item);
                return null;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "UpdateProfile threw an exception.");
                throw ex;
            }
        }

        // Search for anything in filter - eg. { Body: 'something' }
        public async Task<Profile> GetProfileByFilter(string filter)
        {
            try
            {
                return await _context.Profiles
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                _logger.LogWarning(ex, "GetProfileByFilter threw an exception.");
                throw ex;
            }
        }
    }
}
