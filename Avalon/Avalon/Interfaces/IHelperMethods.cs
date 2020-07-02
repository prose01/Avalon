using Avalon.Model;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IHelperMethods
    {
        Task<CurrentUser> GetCurrentUserProfile(ClaimsPrincipal user);

        string GetCurrentUserAuth0Id(ClaimsPrincipal user);

        Task<IEnumerable<string>> DeleteProfile(string profileId);
    }
}
