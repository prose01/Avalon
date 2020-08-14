﻿using Avalon.Infrastructure;
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
            if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

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
            if (profile == null) throw new ArgumentException($"Profile is null.", nameof(profile));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

            if (currentUser.ProfileId == profile.ProfileId) throw new ArgumentException($"Current user cannot set admin status to itself.", nameof(profile));

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
            if (profile == null) throw new ArgumentException($"Profile is null.", nameof(profile));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

            if (currentUser.ProfileId == profile.ProfileId) throw new ArgumentException($"Current user cannot remove admin status from itself.", nameof(profile));

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
        public async Task<ActionResult<Profile>> Get(string profileId)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId == profileId) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileId));

            var profile = await _profilesQueryRepository.GetProfileById(profileId);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        /// <summary>Gets the currentUser's chatMember profiles.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetChatMemberProfiles")]
        public async Task<IEnumerable<Profile>> GetChatMemberProfiles()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profilesQueryRepository.GetChatMemberProfiles(currentUser) ?? throw new ArgumentException($"There are no ChatMembers for current user.");
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

            return await _profilesQueryRepository.GetProfileByFilter(currentUser, profileFilter) ?? throw new ArgumentException($"Current users profileFilter cannot find any matching profiles.", nameof(profileFilter));
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

            if (currentUser.ProfileFilter == null) throw new ArgumentException($"Current users profileFilter is null.");

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
