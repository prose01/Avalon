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

        public async Task<IEnumerable<Profile>> GetAllProfiles(Profile currentUser)
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
                throw ex;
            }
        }

        public async Task<DeleteResult> RemoveProfile(string profileId)
        {
            try
            {
                //return await _context.Profiles.DeleteOneAsync(
                //    Builders<Profile>.Filter.Eq("ProfileId", profileId));

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<ReplaceOneResult> UpdateProfileDocument(Profile item)
        {
            try
            {
                //return await _context.Profiles
                //            .ReplaceOneAsync(p => p.ProfileId.Equals(item.ProfileId)
                //                            , item
                //                            , new UpdateOptions { IsUpsert = true });

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Demo function - full document update
        public async Task<ReplaceOneResult> UpdateProfile(Profile item)
        {
            try
            {
                item.UpdatedOn = DateTime.Now;

                return await UpdateProfileDocument(item);
            }
            catch (Exception ex)
            {
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
                throw ex;
            }
        }

        public async Task<Profile> GetCurrentProfileByEmail(string email)
        {
            var filter = Builders<Profile>.Filter.Eq("Email", email);

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
    }
}
