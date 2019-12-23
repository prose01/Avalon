using Avalon.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfileRepository
    {
        Task<IEnumerable<CurrentUser>> GetAllProfiles(CurrentUser currentUser);
        Task<CurrentUser> GetProfileById(string profileId);
        Task<CurrentUser> GetProfileByName(string profileName);
        Task AddProfile(CurrentUser item);
        Task<DeleteResult> RemoveProfile(string profileId);
        Task<ReplaceOneResult> UpdateProfile(CurrentUser item);
        Task<CurrentUser> GetProfileByFilter(string filter);
        Task<CurrentUser> GetCurrentProfileByEmail(string email);
        Task<CurrentUser> AddProfilesToBookmarks(CurrentUser currentUser, string[] profileIds);
        Task<CurrentUser> RemoveProfilesFromBookmarks(CurrentUser currentUser, string[] profileIds);
    }
}
