using Avalon.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfileRepository
    {
        Task<IEnumerable<Profile>> GetAllProfiles(Profile currentUser);
        Task<Profile> GetProfileById(string profileId);
        Task<Profile> GetProfileByName(string profileName);
        Task AddProfile(Profile item);
        Task<DeleteResult> RemoveProfile(string profileId);
        Task<ReplaceOneResult> UpdateProfile(Profile item);
        Task<Profile> GetProfileByFilter(string filter);
        Task<Profile> GetCurrentProfileByEmail(string email);
        Task<Profile> AddProfilesToBookmarks(Profile currentUser, string[] profileIds);
        Task<Profile> RemoveProfilesFromBookmarks(Profile currentUser, string[] profileIds);
    }
}
