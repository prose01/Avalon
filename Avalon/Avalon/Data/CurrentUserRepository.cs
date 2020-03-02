using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class CurrentUserRepository : ICurrentUserRepository
    {
        private readonly ProfileContext _context = null;

        public CurrentUserRepository(IOptions<Settings> settings)
        {
            _context = new ProfileContext(settings);
        }        

        /// <summary>Adds a new profile.</summary>
        /// <param name="item">The profile.</param>
        public async Task AddProfile(CurrentUser item)
        {
            try
            {
                item.ProfileId = Guid.NewGuid().ToString();
                item.CreatedOn = DateTime.Now;
                item.UpdatedOn = DateTime.Now;
                item.LastActive = DateTime.Now;

                //await _context.CurrentUser.InsertOneAsync(item);
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

                return await _context.CurrentUser
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

        ///// <summary>Gets the current profile by email.</summary>
        ///// <param name="email">The email.</param>
        ///// <returns></returns>
        //public async Task<CurrentUser> GetCurrentProfileByEmail(string email)
        //{
        //    var filter = Builders<CurrentUser>.Filter.Eq("Email", email);

        //    try
        //    {
        //        return await _context.CurrentUser
        //            .Find(filter)
        //            .FirstOrDefaultAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        /// <summary>Gets the current profile by auth0Id.</summary>
        /// <param name="auth0Id">The Auth0Id.</param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentProfileByAuth0Id(string auth0Id)
        {
            var filter = Builders<CurrentUser>.Filter.Eq("Auth0Id", auth0Id);

            try
            {
                return await _context.CurrentUser
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

                return await _context.CurrentUser.FindOneAndUpdateAsync(filter, update, options);
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

                return await _context.CurrentUser.FindOneAndUpdateAsync(filter, update, options);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
