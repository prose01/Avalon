using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [Authorize]
    [ApiController]
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
        /// Gets all Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetAllProfiles/")]
        public async Task<IEnumerable<Profile>> GetAllProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetAllProfiles(currentUser);
        }

        // GET api/profiles/5
        /// <summary>
        /// Gets the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileById/{profileId}")]
        public async Task<Profile> Get(string profileId)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId == profileId) return null;

            return await _profilesQueryRepository.GetProfileById(profileId) ?? null;
        }

        /// <summary>
        /// Gets the specified profile based on a filter. Eg. { Body: 'something' }
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetProfileByFilter")]
        public async Task<Profile> GetProfileByFilter([FromBody]Profile profileFilter)
        {
            return await _profilesQueryRepository.GetProfileByFilter(profileFilter) ?? null; // Should be null if no filter match.
        }

        /// <summary>
        /// Gets Latest Created Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetLatestProfiles/")]
        public async Task<IEnumerable<Profile>> GetLatestProfiles()
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
        public async Task<IEnumerable<Profile>> GetLastActiveProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetLastActiveProfiles(currentUser);
        }

        /// <summary>
        /// Gets Bookmarked Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetBookmarkedProfiles/")]
        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetBookmarkedProfiles(currentUser);
        }
    }
}
