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
            if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

            try
            {
                foreach (var profileId in profileIds)
                {
                    await _helper.DeleteProfileFromAuth0(profileId);
                    await _profilesQueryRepository.DeleteProfile(profileId);
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
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId == profileId) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileId));

            var profile = await _profilesQueryRepository.GetProfileById(profileId);

            if (profile == null)
            {
                return NotFound();
            }

            // Notify profile that currentUser has visited their profile.
            await _profilesQueryRepository.AddVisitedToProfiles(currentUser, profile);

            return profile;
        }

        /// <summary>Gets the currentUser's chatMember profiles.</summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetChatMemberProfiles")]
        public async Task<IEnumerable<Profile>> GetChatMemberProfiles([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            return await _profilesQueryRepository.GetChatMemberProfiles(currentUser, skip, parameterFilter.PageSize) ?? throw new ArgumentException($"There are no ChatMembers for current user.");
        }

        /// <summary>
        /// Gets the specified profile based on a filter. Eg. { Body: 'something' }
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetProfileByFilter")]
        public async Task<IEnumerable<Profile>> GetProfileByFilter([FromBody] RequestBody requestBody, [FromQuery] ParameterFilter parameterFilter)
        {
            if (requestBody.ProfileFilter == null) throw new ArgumentException($"ProfileFilter is null.", nameof(requestBody.ProfileFilter));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            // TODO: skip & limit should either come from RequestBody or ParameterFilter

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            return await _profilesQueryRepository.GetProfileByFilter(currentUser, requestBody.ProfileFilter, requestBody.OrderByType, skip, parameterFilter.PageSize) ?? throw new ArgumentException($"Current users profileFilter cannot find any matching profiles.", nameof(requestBody.ProfileFilter));
        }

        /// <summary>Gets the specified profiles based on the CurrentUser's filter.</summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileByCurrentUsersFilter/")]
        public async Task<IEnumerable<Profile>> GetProfileByCurrentUsersFilter([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileFilter == null) throw new ArgumentException($"Current users profileFilter is null.");

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            return await _profilesQueryRepository.GetProfileByFilter(currentUser, currentUser.ProfileFilter, parameterFilter.OrderByType, skip, parameterFilter.PageSize);
        }

        /// <summary>
        /// Gets Latest Profiles OrderBy.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetLatestProfiles/")]
        public async Task<IEnumerable<Profile>> GetLatestProfiles([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            return await _profilesQueryRepository.GetLatestProfiles(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);
        }

        /// <summary>
        /// Gets Bookmarked Profiles.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetBookmarkedProfiles/")]
        public async Task<IEnumerable<Profile>> GetBookmarkedProfiles([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            return await _profilesQueryRepository.GetBookmarkedProfiles(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);
        }

        #region Maintenance

        /// <summary>Deletes 10 old profiles that are more than 30 days since last active.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpDelete("~/DeleteOldProfiles")]
        public async Task<IActionResult> DeleteOldProfiles()
        {
            try
            {
                var oldProfiles = await _profilesQueryRepository.GetOldProfiles();

                foreach (var profile in oldProfiles)
                {
                    //await _helper.DeleteProfileFromAuth0(profile.ProfileId);
                    //await _profilesQueryRepository.DeleteProfile(profile.ProfileId);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
