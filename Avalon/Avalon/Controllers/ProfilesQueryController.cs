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
    [Route("[controller]")]
    [Authorize]
    public class ProfilesQueryController : Controller
    {
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly IHelperMethods _helper;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, IHelperMethods helperMethods)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _helper = helperMethods;
        }

        /// <summary>
        /// Gets Latest Created Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public async Task<IEnumerable<CurrentUser>> GetLatestProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetLatestCreatedProfiles(currentUser);
        }

        /// <summary>
        /// Gets Last Active Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetLastActiveProfiles/")]
        public async Task<IEnumerable<CurrentUser>> GetLastActiveProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetLastActiveProfiles(currentUser);
        }

        /// <summary>
        /// Gets Bookmarked Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/ProfilesQuery/GetBookmarkedProfiles/")]
        public async Task<IEnumerable<CurrentUser>> GetBookmarkedProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetBookmarkedProfiles(currentUser);
        }
    }
}
