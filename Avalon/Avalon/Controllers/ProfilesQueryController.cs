using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IImageUtil _imageUtil;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, IHelperMethods helperMethods, IImageUtil imageUtil)
        {
            _profilesQueryRepository = profilesQueryRepository;
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

        /// <summary>
        /// Sets the specified profile identifier as admin.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/SetAsAdmin")]
        public async Task<IActionResult> SetAsAdmin([FromBody] Profile profile)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profile == null) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) return BadRequest();

            if (currentUser.ProfileId == profile.ProfileId) return BadRequest();

            return Ok(_profilesQueryRepository.SetAsAdmin(profile.ProfileId));
        }

        /// <summary>
        /// Removes the specified profile identifier as admin.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveAdmin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] Profile profile)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profile == null) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) return BadRequest();

            if (currentUser.ProfileId == profile.ProfileId) return BadRequest();

            return Ok(_profilesQueryRepository.RemoveAdmin(profile.ProfileId));
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

        //[NoCache]
        //[HttpGet("~/GetProfilesById/{profileIds}")]
        //public async Task<IEnumerable<Profile>> Get(string[] profileIds)
        //{
        //    var currentUser = await _helper.GetCurrentUserProfile(User);

        //    if (profileIds.Contains(currentUser.ProfileId)) return null;

        //    return await _profilesQueryRepository.GetProfilesById(profileIds) ?? null;
        //}

        /// <summary>Gets the currentUser's chatMember profiles.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetChatMemberProfiles")]
        public async Task<IEnumerable<Profile>> GetChatMemberProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetChatMemberProfiles(currentUser) ?? null;
        }

        /// <summary>Gets all images from specified profileId.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileImages/{profileId}")]
        public async Task<IActionResult> GetProfileImages(string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) return BadRequest();

                return Ok(await _imageUtil.GetImagesAsync(profileId));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Gets an images from Profile by Image fileName.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="fileName">The image fileName.</param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileImageByFileName/{profileId},{fileName}")]
        public async Task<IActionResult> GetProfileImageByFileName(string profileId, string fileName)
        {
            try
            {
                return Ok(await _imageUtil.GetImageByFileName(profileId, fileName));
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
        /// Gets the specified profiles based on the CurrentUser's filter.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileByCurrentUsersFilter")]
        public async Task<IEnumerable<Profile>> GetProfileByCurrentUsersFilter()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetProfileByFilter(currentUser.ProfileFilter); 
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
