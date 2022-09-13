using Avalon.Model;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface ICurrentUserRepository
    {
        Task AddProfile(CurrentUser item);
        Task<DeleteResult> DeleteCurrentUser(string profileId);
        Task UpdateProfile(CurrentUser item);
        Task<CurrentUser> GetCurrentProfileByAuth0Id(string auth0Id);
        Task SaveProfileFilter(CurrentUser currentUser, ProfileFilter profileFilter);
        Task<ProfileFilter> LoadProfileFilter(CurrentUser currentUser);
        Task AddProfilesToBookmarks(CurrentUser currentUser, string[] profileIds);
        Task RemoveProfilesFromBookmarks(CurrentUser currentUser, string[] profileIds);
        Task AddProfilesToChatMemberslist(CurrentUser currentUser, string[] profileIds);
        Task RemoveProfilesFromChatMemberslist(CurrentUser currentUser, string[] profileIds);
        Task BlockChatMembers(CurrentUser currentUser, string[] profileIds);
        Task CleanCurrentUser(CurrentUser currentUser);
    }
}
