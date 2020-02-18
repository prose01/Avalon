using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalon.Helpers
{
    public class HelperMethods : IHelperMethods
    {
        private readonly ICurrentUserRepository _profileRepository;
        private readonly string _claimsEmail;

        public HelperMethods(IOptions<Settings> settings, ICurrentUserRepository profileRepository)
        {
            _profileRepository = profileRepository;
            _claimsEmail = settings.Value.ClaimsEmail;
        }

        /// <summary>Gets the current user profile.</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentUserProfile(ClaimsPrincipal user)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == _claimsEmail)?.Value;

            return await _profileRepository.GetCurrentProfileByEmail(email) ?? new CurrentUser(); // Burde smide en fejl hvis bruger ikke findes.
        }

        /// <summary>Gets the current user email.</summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public string GetCurrentUserEmail(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == _claimsEmail)?.Value;
        }
    }
}
