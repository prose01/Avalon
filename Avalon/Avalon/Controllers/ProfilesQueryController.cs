using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ProfilesQueryController : Controller
    {
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly ILogger<ProfilesQueryController> _logger;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, ILogger<ProfilesQueryController> logger)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _logger = logger;
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
    }
}
