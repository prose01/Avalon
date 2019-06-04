using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IHelperMethods _helper;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, ILogger<ProfilesQueryController> logger, IHelperMethods helperMethods)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _logger = logger;
            _helper = helperMethods;
        }

        /// <summary>
        /// Gets Latest Created Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public async Task<IEnumerable<Profile>> GetLatestProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            _logger.LogInformation("GetLatestProfiles Information Log.");
            return await GetLatestProfilesInternal(currentUser);
        }

        private async Task<IEnumerable<Profile>> GetLatestProfilesInternal(Profile currentUser)
        {
            return await _profilesQueryRepository.GetLatestProfiles(currentUser);
        }

        /// <summary>
        /// Gets Last Active Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/api/GetLastActiveProfiles/")]
        public async Task<IEnumerable<Profile>> GetLastActiveProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            _logger.LogInformation("GetLastActiveProfiles Information Log.");
            return await GetLastActiveProfilesInternal(currentUser);
        }

        private async Task<IEnumerable<Profile>> GetLastActiveProfilesInternal(Profile currentUser)
        {
            return await _profilesQueryRepository.GetLastActiveProfiles(currentUser);
        }

        /// <summary>
        /// Gets Bookmarked Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/api/GetBookmarkedProfiles/")]
        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles(string profileId)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            _logger.LogInformation("GetBookmarkedProfiles Information Log.");
            return await GetBookmarkedProfilesInternal(currentUser, profileId);
        }

        private async Task<IEnumerable<Profile>> GetBookmarkedProfilesInternal(Profile currentUser, string profileId)
        {
            return await _profilesQueryRepository.GetBookmarkedProfiles(currentUser, profileId);
        }
    }
}
