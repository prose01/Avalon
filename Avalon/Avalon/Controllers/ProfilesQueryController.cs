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
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public Task<IEnumerable<Profile>> GetLatestProfiles()
        {
            _logger.LogInformation("GetLatestProfiles Information Log.");
            return GetGetLatestProfilesInternal();
        }

        private async Task<IEnumerable<Profile>> GetGetLatestProfilesInternal()
        {
            return await _profilesQueryRepository.GetLatestProfiles();
        }

        /// <summary>
        /// Gets oldest Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/api/OldestProfiles/")]
        public Task<IEnumerable<Profile>> GetOldestProfiles()
        {
            return GetOldestProfilesInternal();
        }

        private async Task<IEnumerable<Profile>> GetOldestProfilesInternal()
        {
            return await _profilesQueryRepository.GetLatestProfiles();
        }
    }
}
