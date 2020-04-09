using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly ICurrentUserRepository _profileRepository;
        private readonly IHelperMethods _helper;
        private readonly IImageUtil _imageUtil;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, ICurrentUserRepository profileRepository, IHelperMethods helperMethods, IImageUtil imageUtil)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _profileRepository = profileRepository;
            _helper = helperMethods;
            _imageUtil = imageUtil;
        }

        /// <summary>
        /// Deletes the specified profile identifiers.
        /// </summary>
        /// <param name="profileIds">The profile identifiers.</param>
        [NoCache]
        [HttpPost("~/DeleteProfiles")]
        public async Task<IActionResult> DeleteProfiles([FromBody]string[] profileIds)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) return BadRequest();

            // Delete from Auth0

            return Ok(_profilesQueryRepository.DeleteProfiles(profileIds));
        }
        
        // GET api/profiles/5
        /// <summary>
        /// Gets the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/SetAsAdmin/{profileId},{isAdmin}")]
        public async Task<IActionResult> SetAsAdmin(string profileId, bool isAdmin)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) return BadRequest();

            if (currentUser.ProfileId == profileId) return BadRequest();

            var profile = await _profilesQueryRepository.GetProfileById(profileId) ?? null;

            if (profile == null) return BadRequest();

            profile.Admin = isAdmin;

            return Ok(_profilesQueryRepository.SetAsAdmin(profile));
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

        /// <summary>Gets all images from specified profileId.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileImages/{profileId}")]
        public async Task<IActionResult> GetProfileImages(string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) return BadRequest();

                var images = await _imageUtil.GetImagesAsync(profileId);

                return Ok(images);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the specified profile based on a filter. Eg. { Body: 'something' }
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetProfileByFilter")]
        public async Task<IEnumerable<Profile>> GetProfileByFilter([FromBody]ProfileFilter profileFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);
            profileFilter.CurrentUserId = currentUser.ProfileId;

            return await _profilesQueryRepository.GetProfileByFilter(profileFilter) ?? null; // Should be null if no filter match.
        }

        /// <summary>
        /// Gets Latest Created Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetLatestCreatedProfiles/")]
        public async Task<IEnumerable<Profile>> GetLatestCreatedProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetLatestCreatedProfiles(currentUser);
        }

        /// <summary>
        /// Gets Latest Created Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetLastUpdatedProfiles/")]
        public async Task<IEnumerable<Profile>> GetLastUpdatedProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetLastUpdatedProfiles(currentUser);
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
