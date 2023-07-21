using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Configuration;
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
        private readonly Context _context = null;
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private int _complainsDaysBack;
        private int _complainsWarnUser;

        public CurrentUserRepository(IOptions<Settings> settings, IConfiguration config, IProfilesQueryRepository profilesQueryRepository)
        {
            _context = new Context(settings);
            _profilesQueryRepository = profilesQueryRepository;
            _complainsDaysBack = config.GetValue<int>("ComplainsDaysBack");
            _complainsWarnUser = config.GetValue<int>("ComplainsWarnUser");
        }

        /// <summary>Adds a new profile.</summary>
        /// <param name="currentUser">The current user.</param>
        public async Task AddProfile(CurrentUser currentUser)
        {
            try
            {
                currentUser.ProfileId = Guid.NewGuid().ToString();
                currentUser.CreatedOn = DateTime.UtcNow;
                currentUser.UpdatedOn = DateTime.UtcNow;
                currentUser.LastActive = DateTime.UtcNow;

                await _context.CurrentUser.InsertOneAsync(currentUser);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Deletes the profile.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteCurrentUser(string profileId)
        {
            try
            {
                return await _context.CurrentUser.DeleteOneAsync(
                    Builders<CurrentUser>.Filter.Eq("ProfileId", profileId));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Updates the profile.</summary>
        /// <param name="currentUser">The current user.</param>
        public async Task UpdateProfile(CurrentUser currentUser)
        {
            try
            {
                currentUser.UpdatedOn = DateTime.UtcNow;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                await _context.CurrentUser.ReplaceOneAsync(filter, currentUser);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the current profile by Auth0Id.</summary>
        /// <param name="auth0Id">The Auth0Id.</param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentProfileByAuth0Id(string auth0Id)
        {
            try
            {
                var filter = Builders<CurrentUser>.Filter.Eq("Auth0Id", auth0Id);

                var update = Builders<CurrentUser>
                                .Update.Set(c => c.LastActive, DateTime.UtcNow);

                var options = new FindOneAndUpdateOptions<CurrentUser>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return await _context.CurrentUser.FindOneAndUpdateAsync(filter, update, options);

            }
            catch
            {
                throw;
            }
        }

        /// <summary>Saves the profile filter to currentUser.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileFilter">The profile filter.</param>
        public async Task SaveProfileFilter(CurrentUser currentUser, ProfileFilter profileFilter)
        {
            try
            {
                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.Set(c => c.ProfileFilter, profileFilter);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Load the profile filter from currentUser.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public async Task<ProfileFilter> LoadProfileFilter(CurrentUser currentUser)
        {
            try
            {
                var query = from c in _context.CurrentUser.AsQueryable()
                            where c.ProfileId == currentUser.ProfileId
                            select c.ProfileFilter;

                return await Task.FromResult(query.FirstOrDefault());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Adds the profiles to currentUser bookmarks.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task AddProfilesToBookmarks(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out already bookmarked profiles.
                var newBookmarks = profileIds.Where(i => !currentUser.Bookmarks.Contains(i)).ToList();

                if (newBookmarks.Count == 0)
                    return;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PushEach(c => c.Bookmarks, newBookmarks);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Removes the profiles from currentUser bookmarks.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task RemoveProfilesFromBookmarks(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out already removed bookmarked profiles.
                var removeBookmarks = profileIds.Where(i => currentUser.Bookmarks.Contains(i)).ToList();

                if (removeBookmarks.Count == 0)
                    return;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PullAll(c => c.Bookmarks, removeBookmarks);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Adds the profiles to currentUser ChatMemberslist.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task AddProfilesToChatMemberslist(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out already added ChatMembers.
                var newChatMemberIds = profileIds.Where(i => !currentUser.ChatMemberslist.Any(m => m.ProfileId == i)).ToList();

                if (newChatMemberIds.Count == 0)
                    return;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                List<ChatMember> newChatMembers = new List<ChatMember>();

                foreach (var chatMemberId in newChatMemberIds)
                {
                    var chatMember = this._profilesQueryRepository.GetProfileById(chatMemberId).Result;

                    // Uncontactable profiles are not added to chatmemberlist unless currentUser is admin.
                    if (!chatMember.Contactable && !currentUser.Admin) continue;

                    newChatMembers.Add(new ChatMember() { ProfileId = chatMemberId, Name = chatMember.Name, Avatar = chatMember.Avatar, Blocked = false });
                }

                var update = Builders<CurrentUser>
                                .Update.PushEach(c => c.ChatMemberslist, newChatMembers);      // TODO: Kig på $addToSet

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Removes the profiles from currentUser ChatMemberslist.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        public async Task RemoveProfilesFromChatMemberslist(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                //Filter out ChatMembers not on list.
                if (currentUser.ChatMemberslist.Where(i => profileIds.Contains(i.ProfileId)).ToList().Count == 0)
                {
                    return;
                }

                foreach (var profileId in profileIds)
                {
                    currentUser.ChatMemberslist.RemoveAll(i => i.ProfileId == profileId);
                }

                var filter = Builders<CurrentUser>
                                    .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                            .Update.Set(c => c.ChatMemberslist, currentUser.ChatMemberslist);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Add group to CurrentUser.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="groupId">The group id.</param>
        public async Task AddGroupToCurrentUser(CurrentUser currentUser, string groupId)
        {
            try
            {
                //Check if currentUser has already joined group.
                if(currentUser.Groups.Contains(groupId))
                {
                    return;
                }

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.Push(c => c.Groups, groupId);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Remove groups from CurrentUser.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="groupIds">The group ids.</param>
        public async Task RemoveGroupsFromCurrentUser(CurrentUser currentUser, string[] groupIds)
        {
            try
            {
                //Filter out already removed groups.
                var removeGroups = groupIds.Where(i => currentUser.Groups.Contains(i)).ToList();

                if (removeGroups.Count == 0)
                    return;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                                .Update.PullAll(c => c.Groups, removeGroups);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Blocks or unblocks chatmember profiles.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile identifiers.</param>
        /// <returns></returns>
        public async Task BlockChatMembers(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                List<string> updatableChatMembers = profileIds.Where(i => currentUser.ChatMemberslist.Any(m => m.ProfileId == i)).ToList();

                if (updatableChatMembers.Count == 0)
                    return;

                var filter = Builders<CurrentUser>
                                .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                List<ChatMember> updateChatMembers = new List<ChatMember>();

                foreach (var member in currentUser.ChatMemberslist)
                {
                    if (profileIds.Any(m => member.ProfileId == m))
                    {
                        member.Blocked = !member.Blocked;
                    }

                    updateChatMembers.Add(member);
                }

                var update = Builders<CurrentUser>
                                .Update.Set(c => c.ChatMemberslist, updateChatMembers);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Clean CurrentUser for obsolete profile info.</summary>
        /// <param name="currentUser">The current user.</param>
        public async Task CleanCurrentUser(CurrentUser currentUser)
        {
            try
            {
                // Clean CurrentUser for obsolete profile info.

                string[] checkThesesProfiles = currentUser.Visited.Keys.ToArray<string>();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.Bookmarks).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.IsBookmarked.Keys).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.ChatMemberslist.Select(i => i.ProfileId)).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.Likes).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.Complains.Keys).ToArray();


                // Chech if our profiles exist
                IEnumerable<Profile> exitingprofiles = await this._profilesQueryRepository.GetProfilesByIds(checkThesesProfiles);

                //List<string> newCountrycode = new List<string>();
                List<string> exitingIds = new List<string>();
                foreach (var profile in exitingprofiles)
                {
                    // Remove all profiles not in currentUser.Countrycode i.e. if they have changed country.
                    if (profile.Countrycode == currentUser.Countrycode)
                    {
                        exitingIds.Add(profile.ProfileId);
                    }
                }


                var deleteTheseProfiles = checkThesesProfiles.Where(i => !exitingIds.Contains(i)).ToList();

                if (deleteTheseProfiles.Count > 0)
                {
                    foreach (var deadProfile in deleteTheseProfiles)
                    {
                        if (currentUser.Visited.ContainsKey(deadProfile))
                        {
                            currentUser.Visited.Remove(deadProfile);
                        }

                        if (currentUser.Bookmarks.Contains(deadProfile))
                        {
                            currentUser.Bookmarks.Remove(deadProfile);
                        }

                        if (currentUser.IsBookmarked.ContainsKey(deadProfile))
                        {
                            currentUser.IsBookmarked.Remove(deadProfile);
                        }

                        if (currentUser.ChatMemberslist.Any(i => i.ProfileId == deadProfile))
                        {
                            currentUser.ChatMemberslist.RemoveAll(i => i.ProfileId == deadProfile);
                        }

                        if (currentUser.Likes.Contains(deadProfile))
                        {
                            currentUser.Likes.Remove(deadProfile);
                        }

                        if (currentUser.IsBookmarked.ContainsKey(deadProfile))
                        {
                            currentUser.IsBookmarked.Remove(deadProfile);
                        }

                        if (currentUser.Complains.ContainsKey(deadProfile))
                        {
                            currentUser.Complains.Remove(deadProfile);
                        }
                    }

                    var filter = Builders<CurrentUser>
                                    .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                    await _context.CurrentUser.ReplaceOneAsync(filter, currentUser);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Check if CurrentUser has too many complains.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns>Returns true if user should get a warning.</returns>
        public async Task<bool> CheckForComplains(CurrentUser currentUser)
        {
            try
            {
                // Remove old complains.
                var oldcomplains = currentUser.Complains.Where(i => i.Value < DateTime.UtcNow.AddDays(-_complainsDaysBack)).ToArray();

                if (oldcomplains.Length > 0)
                {
                    foreach (var oldcomplain in oldcomplains)
                    {
                        currentUser.Complains.Remove(oldcomplain.Key.ToString());
                    }

                    var filter = Builders<CurrentUser>
                                    .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                    await _context.CurrentUser.ReplaceOneAsync(filter, currentUser);
                }

                // Check if CurrentUser has too many complains and should receive a warning.
                if (currentUser.Complains.Count >= _complainsWarnUser)
                {
                    return true;
                }

                return false;

            }
            catch
            {
                throw;
            }
        }
    }
}
