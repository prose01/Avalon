using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfilesQueryRepository
    {
        Task<IEnumerable<Profile>> GetLatestProfiles(Profile currentUser);

        Task<IEnumerable<Profile>> GetLastActiveProfiles(Profile currentUser);

        Task<IEnumerable<Profile>> GetBookmarkedProfiles(Profile currentUser, string profileId);
    }
}
