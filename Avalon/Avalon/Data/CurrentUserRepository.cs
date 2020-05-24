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
    public class CurrentUserRepository : ICurrentUserRepository
    {
        private readonly ProfileContext _context = null;

        public CurrentUserRepository(IOptions<Settings> settings)
        {
            _context = new ProfileContext(settings);
        }        

        /// <summary>Adds a new profile.</summary>
        /// <param name="item">The profile.</param>
        public async Task AddProfile(CurrentUser currentUser)
        {
            try
            {
                currentUser.ProfileId = Guid.NewGuid().ToString();
                currentUser.CreatedOn = DateTime.Now;
                currentUser.UpdatedOn = DateTime.Now;
                currentUser.LastActive = DateTime.Now;

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
        public async Task<DeleteResult> DeleteCurrentUser(string profileId)
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
        public async Task<ReplaceOneResult> UpdateProfile(CurrentUser currentUser)
        {
            try
            {
                currentUser.UpdatedOn = DateTime.Now;

                return await _context.CurrentUser
                            .ReplaceOneAsync(p => p.ProfileId.Equals(currentUser.ProfileId)
                                            , currentUser
                                            , new UpdateOptions { IsUpsert = true });

                //return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

        /// <summary>Adds the profiles to currentUser ChatMemberslist.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task<CurrentUser> AddProfilesToChatMemberslist(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out allready added ChatMembers.
                var newChatMemberIds = profileIds.Where(i => !currentUser.ChatMemberslist.Any(n => n.ProfileId == i)).ToList();

                if (newChatMemberIds.Count == 0)
                    return null;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                List<ChatMember> newChatMembers = new List<ChatMember>();

                foreach (var chatMemberId in newChatMemberIds)
                {
                    newChatMembers.Add(new ChatMember() { ProfileId = chatMemberId, Blocked = false });
                }

                var update = Builders<CurrentUser>
                                .Update.PushEach(e => e.ChatMemberslist, newChatMembers);      // TODO: Kig på $addToSet

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

        /// <summary>Removes the profiles from currentUser ChatMemberslist.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task<CurrentUser> RemoveProfilesFromChatMemberslist(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out ChatMembers not on list.
                var removeChatMemberIds = profileIds.Where(i => currentUser.ChatMemberslist.Any(n => n.ProfileId == i)).ToList();

                if (removeChatMemberIds.Count == 0)
                    return null;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                List<ChatMember> removeChatMembers = new List<ChatMember>();

                foreach (var chatMemberId in removeChatMemberIds)
                {
                    removeChatMembers.Add(new ChatMember() { ProfileId = chatMemberId, Blocked = false });
                }

                var update = Builders<CurrentUser>
                                .Update.PullAll(e => e.ChatMemberslist, removeChatMembers);

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

        public async Task<CurrentUser> BlockChatMembers(CurrentUser currentUser, List<ChatMember> chatMembers)
        {
            try
            {
                var updatableChatMembers = chatMembers.Where(i => currentUser.ChatMemberslist.Any(n => n.ProfileId == i.ProfileId)).ToList();

                if (updatableChatMembers.Count == 0)
                    return null;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PushEach(e => e.ChatMemberslist, updatableChatMembers);

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

        /// <summary>Adds the image to profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        public async Task<CurrentUser> AddImageToCurrentUser(CurrentUser currentUser, string fileName, string title)
        {
            try
            {
                var imageModel = new ImageModel() { ImageId = Guid.NewGuid().ToString(), FileName = fileName, Title = title };

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.Push(e => e.Images, imageModel);

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

        /// <summary>Removes the image from profile.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="id">The image identifier.</param>
        /// <returns></returns>
        public async Task<CurrentUser> RemoveImageFromCurrentUser(CurrentUser currentUser, string id)
        {
            try
            {
                var images = currentUser.Images.Where(i => i.ImageId == id).ToList();

                var filter = Builders<CurrentUser>
                                .Filter.Eq(e => e.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PullAll(e => e.Images, images);

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
