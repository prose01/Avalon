using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfilesQueryRepository
    {
        Task<IEnumerable<Profile>> GetLatestCreatedProfiles(Profile currentUser);

        Task<IEnumerable<Profile>> GetLastUpdatedProfiles(Profile currentUser);

        Task<IEnumerable<Profile>> GetLastActiveProfiles(Profile currentUser);

        Task<IEnumerable<Profile>> GetBookmarkedProfiles(Profile currentUser);

        //Task<IEnumerable<Profile>> GetBookmarkedLatestCreatedProfiles(Profile currentUser);

        //Task<IEnumerable<Profile>> GetBookmarkedLastUpdatedProfiles(Profile currentUser);

        //Task<IEnumerable<Profile>> GetBookmarkedLastActiveProfiles(Profile currentUser);
    }
}
