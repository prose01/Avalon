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
        private readonly Context _context = null;
        private readonly IProfilesQueryRepository _profilesQueryRepository;

        public CurrentUserRepository(IOptions<Settings> settings, IProfilesQueryRepository profilesQueryRepository)
        {
            _context = new Context(settings);
            _profilesQueryRepository = profilesQueryRepository;
        }

        /// <summary>Adds a new profile.</summary>
        /// <param name="currentUser">The current user.</param>
        public async Task AddProfile(CurrentUser currentUser)
        {
            try
            {
                currentUser.ProfileId = Guid.NewGuid().ToString();
                currentUser.CreatedOn = DateTime.Now;
                currentUser.UpdatedOn = DateTime.Now;
                currentUser.LastActive = DateTime.Now;

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
                currentUser.UpdatedOn = DateTime.Now;

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
                                .Update.Set(c => c.LastActive, DateTime.Now);

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
                //Filter out allready bookmarked profiles.
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
                //Filter out allready bookmarked profiles.
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
                //Filter out allready added ChatMembers.
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

                    newChatMembers.Add(new ChatMember() { ProfileId = chatMemberId, Name = chatMember.Name, Blocked = false });
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
                if(currentUser.ChatMemberslist.Where(i => profileIds.Contains(i.ProfileId)).ToList().Count == 0)
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
                    if(profileIds.Any(m => member.ProfileId == m))
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

        /// <summary> Remove obsolete Profiles from Visited.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile identifiers.</param>
        public async Task RemoveProfilesFromVisited(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                foreach (var profileId in profileIds)
                {
                    if (currentUser.Visited.ContainsKey(profileId))
                    {
                        currentUser.Visited.Remove(profileId);
                    }
                }

                var filter = Builders<CurrentUser>
                           .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                            .Update.Set(c => c.Visited, currentUser.Visited);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary> Remove obsolete Profiles from IsBookmarked.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile identifiers.</param>
        public async Task RemoveProfilesFromIsBookmarked(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                foreach (var profileId in profileIds)
                {
                    if (currentUser.IsBookmarked.ContainsKey(profileId))
                    {
                        currentUser.IsBookmarked.Remove(profileId);
                    }
                }

                var filter = Builders<CurrentUser>
                           .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                            .Update.Set(c => c.IsBookmarked, currentUser.IsBookmarked);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary> Remove obsolete Profiles from Likes.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="profileIds">The profile identifiers.</param>
        public async Task RemoveProfilesFromLikes(CurrentUser currentUser, string[] profileIds)
        {
            try
            {
                foreach (var profileId in profileIds)
                {
                    if (currentUser.Likes.Contains(profileId))
                    {
                        currentUser.Likes.Remove(profileId);
                    }
                }

                var filter = Builders<CurrentUser>
                           .Filter.Eq(c => c.ProfileId, currentUser.ProfileId);

                var update = Builders<CurrentUser>
                            .Update.Set(c => c.Likes, currentUser.Likes);

                await _context.CurrentUser.FindOneAndUpdateAsync(filter, update);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Clean CurrenProfile for obsolete profile info.</summary>
        /// <param name="currentUser">The current user.</param>
        public async Task CleanCurrentUser(CurrentUser currentUser)
        {
            try
            {
                string[] checkThesesProfiles = currentUser.Visited.Keys.ToArray<string>();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.Bookmarks).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.IsBookmarked.Keys).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.ChatMemberslist.Select(i => i.ProfileId)).ToArray();

                checkThesesProfiles = checkThesesProfiles.Union(currentUser.Likes).ToArray();


                // Chech if our profiles exist
                IEnumerable<Profile> exitingprofiles = await this._profilesQueryRepository.GetProfilIdsByIds(checkThesesProfiles);
                
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
    }
}
