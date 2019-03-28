using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfilesQueryController : Controller
    {
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly ILogger<ProfilesQueryController> _logger;
        private readonly string _claimsNickname;
        private readonly string _claimsEmail;

        public ProfilesQueryController(IOptions<Settings> settings, IProfilesQueryRepository profilesQueryRepository, ILogger<ProfilesQueryController> logger)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _logger = logger;
            _claimsNickname = settings.Value.ClaimsNickname;
            _claimsEmail = settings.Value.ClaimsEmail;
        }

        /// <summary>
        /// Gets Latest Created Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public Task<IEnumerable<Profile>> GetLatestProfiles()
        {
            _logger.LogInformation("GetLatestProfiles Information Log.");
            return GetLatestProfilesInternal();
        }

        private async Task<IEnumerable<Profile>> GetLatestProfilesInternal()
        {
            return await _profilesQueryRepository.GetLatestProfiles();
        }

        /// <summary>
        /// Gets Last Active Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/api/GetLastActiveProfiles/")]
        public Task<IEnumerable<Profile>> GetLastActiveProfiles()
        {
            _logger.LogInformation("GetLastActiveProfiles Information Log.");
            return GetLastActiveProfilesInternal();
        }

        private async Task<IEnumerable<Profile>> GetLastActiveProfilesInternal()
        {
            return await _profilesQueryRepository.GetLastActiveProfiles();
        }

        /// <summary>
        /// Gets Bookmarked Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/api/GetBookmarkedProfiles/")]
        public Task<IEnumerable<Profile>> GetBookmarkedProfiles(string profileId)
        {
            _logger.LogInformation("GetBookmarkedProfiles Information Log.");
            return GetBookmarkedProfilesInternal(profileId);
        }

        private async Task<IEnumerable<Profile>> GetBookmarkedProfilesInternal(string profileId)
        {
            return await _profilesQueryRepository.GetBookmarkedProfiles(profileId);
        }
    }
}
