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
        private readonly IHelperMethods _helper;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, IHelperMethods helperMethods)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _helper = helperMethods;
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

            try
            {
                foreach (var profileId in profileIds)
                {
                    await _helper.DeleteProfile(profileId);
                    await _profilesQueryRepository.DeleteProfile(profileId);
                    //_imageUtil.DeleteAllImagesForProfile(currentUser, profileId); // Call Artemis
                }

                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        /// <summary>Gets the currentUser's chatMember profiles.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetChatMemberProfiles")]
        public async Task<IEnumerable<Profile>> GetChatMemberProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetChatMemberProfiles(currentUser) ?? null;
        }

        ///// <summary>Gets all images from specified profileId.</summary>
        ///// <param name="profileId">The profile identifier.</param>
        ///// <returns></returns>
        //[NoCache]
        //[HttpGet("~/GetProfileImages/{profileId}")]
        //public async Task<IActionResult> GetProfileImages(string profileId)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(profileId)) return BadRequest();

        //        return Ok(await _imageUtil.GetImagesAsync(profileId));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        ///// <summary>Gets an images from Profile by Image fileName.</summary>
        ///// <param name="profileId">The profile identifier.</param>
        ///// <param name="fileName">The image fileName.</param>
        ///// <returns></returns>
        //[NoCache]
        //[HttpGet("~/GetProfileImageByFileName/{profileId},{fileName}")]
        //public async Task<IActionResult> GetProfileImageByFileName(string profileId, string fileName)
        //{
        //    try
        //    {
        //        return Ok(await _imageUtil.GetImageByFileName(profileId, fileName));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// Gets the specified profile based on a filter. Eg. { Body: 'something' }
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetProfileByFilter")]
        public async Task<IEnumerable<Profile>> GetProfileByFilter([FromBody]ProfileFilter profileFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetProfileByFilter(currentUser, profileFilter) ?? null; // Should be null if no filter match.
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

            if (currentUser.ProfileFilter == null) return null;     //TODO: Find på noget bedre end null.

            return await _profilesQueryRepository.GetProfileByFilter(currentUser, currentUser.ProfileFilter); 
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
