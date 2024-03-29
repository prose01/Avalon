﻿using Avalon.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfilesQueryRepository
    {
        Task<Profile> SetAsAdmin(string profileId);
        Task<Profile> RemoveAdmin(string profileId);
        Task<DeleteResult> DeleteProfile(string profileId);
        //Task<DeleteResult> DeleteProfiles(string[] profileIds);
        Task<Profile> GetProfileById(string profileId);
        Task<IEnumerable<Profile>> GetProfilesByIds(string[] profileIds);
        Task<IEnumerable<Profile>> GetProfilesByIds(CurrentUser currentUser, string[] profileIds, int skip, int limit);
        //Task<IEnumerable<Profile>> GetChatMemberProfiles(CurrentUser currentUser, int skip, int limit);
        Task<Profile> GetProfileByAuth0Id(string auth0Id);
        //Task<Profile> GetProfileByNameStrict(string profileName);
        //Task<Profile> GetProfileByName(string profileName);
        Task<IEnumerable<Profile>> GetProfileByFilter(CurrentUser currentUser, ProfileFilter profileFilter, OrderByType orderByType, int skip, int limit);
        Task<IEnumerable<Profile>> GetLatestProfiles(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task AddIsBookmarkedToProfiles(CurrentUser currentUser, string[] profileIds);
        Task RemoveIsBookmarkedFromProfiles(CurrentUser currentUser, string[] profileIds);
        Task<IEnumerable<Profile>> GetBookmarkedProfiles(CurrentUser profile, OrderByType orderByType, int skip, int limit);
        Task<IEnumerable<Profile>> GetProfilesWhoVisitedMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task<IEnumerable<Profile>> GetProfilesWhoBookmarkedMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task<IEnumerable<Profile>> GetProfilesWhoLikesMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task AddVisitedToProfiles(CurrentUser currentUser, Profile profile);
        Task AddLikeToProfiles(CurrentUser currentUser, string[] profileIds);
        Task RemoveLikeFromProfiles(CurrentUser currentUser, string[] profileIds);
        Task AddComplainToProfile(CurrentUser currentUser, Profile profile);
        Task<IEnumerable<Profile>> GetOldProfiles(int daysBack, int limit);
    }
}
