using Avalon.Model;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface ICurrentUserRepository
    {
        Task AddProfile(CurrentUser item);
        Task<DeleteResult> DeleteCurrentUser(string profileId);
        Task<ReplaceOneResult> UpdateProfile(CurrentUser item);
        Task<CurrentUser> GetCurrentProfileByAuth0Id(string auth0Id);
        Task<CurrentUser> AddProfilesToBookmarks(CurrentUser currentUser, string[] profileIds);
        Task<CurrentUser> RemoveProfilesFromBookmarks(CurrentUser currentUser, string[] profileIds);
        Task<CurrentUser> AddImageToCurrentUser(CurrentUser currentUser, string fileName, string title);
        Task<CurrentUser> RemoveImageFromCurrentUser(CurrentUser currentUser, string id);
    }
}
