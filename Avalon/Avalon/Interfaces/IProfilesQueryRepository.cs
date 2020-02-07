using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfilesQueryRepository
    {
        Task<IEnumerable<Profile>> GetAllProfiles(CurrentUser currentUser);
        Task<Profile> GetProfileById(string profileId);
        Task<Profile> GetProfileByName(string profileName);
        Task<Profile> GetProfileByFilter(ProfileFilter profileFilter);
        Task<IEnumerable<Profile>> GetLatestCreatedProfiles(CurrentUser profile);
        Task<IEnumerable<Profile>> GetLastUpdatedProfiles(CurrentUser profile);
        Task<IEnumerable<Profile>> GetLastActiveProfiles(CurrentUser profile);
        Task<IEnumerable<Profile>> GetBookmarkedProfiles(CurrentUser profile);
        //Task<IEnumerable<CurrentUser>> GetBookmarkedLatestCreatedProfiles(CurrentUser currentUser);
        //Task<IEnumerable<CurrentUser>> GetBookmarkedLastUpdatedProfiles(CurrentUser currentUser);
        //Task<IEnumerable<CurrentUser>> GetBookmarkedLastActiveProfiles(CurrentUser currentUser);
    }
}
