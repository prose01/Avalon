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
        Task<DeleteResult> DeleteProfiles(string[] profileIds);
        Task<Profile> GetProfileById(string profileId);
        Task<IEnumerable<Profile>> GetChatMemberProfiles(CurrentUser currentUser);
        Task<Profile> GetProfileByAuth0Id(string auth0Id);
        Task<Profile> GetProfileByName(string profileName);
        Task<IEnumerable<Profile>> GetProfileByFilter(CurrentUser currentUser, ProfileFilter profileFilter);
        Task<IEnumerable<Profile>> GetLatestCreatedProfiles(CurrentUser profile);
        Task<IEnumerable<Profile>> GetBookmarkedProfiles(CurrentUser profile);
    }
}
