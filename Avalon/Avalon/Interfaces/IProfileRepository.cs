using Avalon.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfileRepository
    {
        Task<IEnumerable<Profile>> GetAllProfiles(string profileName);
        Task<Profile> GetProfile(string profileId);
        Task<Profile> GetProfileByName(string profileName);
        Task AddProfile(Profile item);
        Task<DeleteResult> RemoveProfile(string profileId);
        Task<ReplaceOneResult> UpdateProfile(string profileId, Profile item);
        Task<Profile> GetProfileByFilter(string filter);
    }
}
