using Avalon.Model;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface ICurrentUserRepository
    {
        Task AddProfile(CurrentUser item);
        Task<DeleteResult> RemoveProfile(string profileId);
        Task<ReplaceOneResult> UpdateProfile(CurrentUser item);
        Task<CurrentUser> GetCurrentProfileByEmail(string email);
        Task<CurrentUser> AddProfilesToBookmarks(CurrentUser currentUser, string[] profileIds);
        Task<CurrentUser> RemoveProfilesFromBookmarks(CurrentUser currentUser, string[] profileIds);
    }
}
