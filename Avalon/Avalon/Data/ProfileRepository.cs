﻿using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Gets all profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<IEnumerable<CurrentUser>> GetAllProfiles(CurrentUser currentUser)
        {
            try
            {
                return await _context.Profiles.Find<CurrentUser>(p => true && p.Email != currentUser.Email).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets the profile by identifier.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        public async Task<CurrentUser> GetProfileById(string profileId)
        {
            var filter = Builders<CurrentUser>
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
        public async Task<CurrentUser> GetProfileByName(string profileName)
        {
            var filter = Builders<CurrentUser>
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

        /// <summary>Adds a new profile.</summary>
        /// <param name="item">The profile.</param>
        public async Task AddProfile(CurrentUser item)
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

        /// <summary>Deletes the profile.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        public async Task<DeleteResult> RemoveProfile(string profileId)
        {
            try
            {
                //return await _context.Profiles.DeleteOneAsync(
                //    Builders<CurrentUser>.Filter.Eq("ProfileId", profileId));

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Updates the profile.</summary>
        /// <param name="item">The profile.</param>
        /// <returns></returns>
        public async Task<ReplaceOneResult> UpdateProfile(CurrentUser item)
        {
            try
            {
                item.UpdatedOn = DateTime.Now;

                return await _context.Profiles
                            .ReplaceOneAsync(p => p.ProfileId.Equals(item.ProfileId)
                                            , item
                                            , new UpdateOptions { IsUpsert = true });

                //return null;
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
        public async Task<CurrentUser> GetProfileByFilter(string filter)
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

        /// <summary>Gets the current profile by email.</summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentProfileByEmail(string email)
        {
            var filter = Builders<CurrentUser>.Filter.Eq("Email", email);

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

        /// <summary>Adds the profiles to currentUser bookmarks.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task<CurrentUser> AddProfilesToBookmarks(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out allready bookmarked profiles.
                var newBookmarks = profileIds.Where(i => !currentUser.Bookmarks.Contains(i)).ToList();

                if (newBookmarks.Count == 0)
                    return null;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PushEach(e => e.Bookmarks, newBookmarks);

                var options = new FindOneAndUpdateOptions<CurrentUser>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return  _context.Profiles.FindOneAndUpdate(filter, update, options);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Removes the profiles from currentUser bookmarks.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task<CurrentUser> RemoveProfilesFromBookmarks(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out allready bookmarked profiles.
                var removeBookmarks = profileIds.Where(i => currentUser.Bookmarks.Contains(i)).ToList();

                if (removeBookmarks.Count == 0)
                    return null;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PullAll(e => e.Bookmarks, removeBookmarks);

                var options = new FindOneAndUpdateOptions<CurrentUser>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return _context.Profiles.FindOneAndUpdate(filter, update, options);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
