using Avalon.Interfaces;
using Avalon.Model;
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

        public ProfileRepository(IOptions<Settings> settings)
        {
            _context = new ProfileContext(settings);
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
                throw ex;
            }
        }

        public async Task AddProfile(Profile item)
        {
            try
            {
                // TODO: create Id
                item.ProfileId = "124";
                item.CreatedOn = DateTime.Now;
                item.UpdatedOn = DateTime.Now;

                await _context.Profiles.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
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
                throw ex;
            }
        }

        private async Task<ReplaceOneResult> UpdateProfileDocument(string profileId, Profile item)
        {
            try
            {
                return await _context.Profiles
                            .ReplaceOneAsync(p => p.ProfileId.Equals(profileId)
                                            , item
                                            , new UpdateOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        //Demo function - full document update
        public async Task<ReplaceOneResult> UpdateProfile(string profileId, Profile item)
        {
            try
            {
                item.UpdatedOn = DateTime.Now;

                return await UpdateProfileDocument(profileId, item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}
