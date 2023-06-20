using Avalon.Model;
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
        Task<Profile> GetProfileByAuth0Id(string auth0Id);
        Task<Profile> GetProfileById(string profileId);
        Task<IEnumerable<Profile>> GetProfilesByIds(string[] profileIds);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetProfilesByIds(CurrentUser currentUser, string[] profileIds, int skip, int limit);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetBookmarkedProfiles(CurrentUser profile, OrderByType orderByType, int skip, int limit);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetProfileByFilter(CurrentUser currentUser, ProfileFilter profileFilter, OrderByType orderByType, int skip, int limit);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetLatestProfiles(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetProfilesWhoVisitedMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetProfilesWhoBookmarkedMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task<(int total, IReadOnlyList<Profile> profiles)> GetProfilesWhoLikesMe(CurrentUser currentUser, OrderByType orderByType, int skip, int limit);
        Task AddIsBookmarkedToProfiles(CurrentUser currentUser, string[] profileIds);
        Task RemoveIsBookmarkedFromProfiles(CurrentUser currentUser, string[] profileIds);
        Task AddVisitedToProfiles(CurrentUser currentUser, Profile profile);
        Task AddLikeToProfiles(CurrentUser currentUser, string[] profileIds);
        Task RemoveLikeFromProfiles(CurrentUser currentUser, string[] profileIds);
        Task AddComplainToProfile(CurrentUser currentUser, Profile profile);
        Task<IEnumerable<Profile>> GetOldProfiles(int daysBack, int limit);
    }
}
