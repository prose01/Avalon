using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IProfilesQueryRepository
    {
        Task<IEnumerable<Profile>> GetLatestProfiles();

        Task<IEnumerable<Profile>> GetLastActiveProfiles();
    }
}
