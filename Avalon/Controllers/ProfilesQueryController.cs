using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
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
        //private readonly bool _deleteOldProfiles;

        public ProfilesQueryController(IProfilesQueryRepository profilesQueryRepository, IHelperMethods helperMethods, IConfiguration config)
        {
            _profilesQueryRepository = profilesQueryRepository;
            _helper = helperMethods;
            //_deleteOldProfiles = config.GetValue<bool>("DeleteOldProfiles");
        }

        /// <summary>
        /// Deletes the specified profile identifiers.
        /// </summary>
        /// <param name="profileIds">The profile identifiers.</param>
        /// <exception cref="ArgumentException">ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}. {profileIds}</exception>
        /// <exception cref="ArgumentException">Current user does not have admin status.</exception>
        [NoCache]
        [HttpPost("~/DeleteProfiles")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteProfiles([FromBody] string[] profileIds)
        {
            try
            {
                if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

                if (Array.Exists(profileIds, x => x == currentUser.ProfileId))
                {
                    throw new ArgumentException($"Admins cannot delete themseleves.");
                }
                else
                {
                    foreach (var profileId in profileIds)
                    {
                        if (profileId == null) continue;

                        await _helper.DeleteProfileFromAuth0(profileId);
                        await _profilesQueryRepository.DeleteProfile(profileId);
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Sets the specified profile identifier as admin.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <exception cref="ArgumentException">Profile is null. {profile}</exception>
        /// <exception cref="ArgumentException">Current user does not have admin status.</exception>
        /// <exception cref="ArgumentException">Current user cannot set admin status to itself. {profile}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/SetAsAdmin")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetAsAdmin([FromBody] string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"Profile is null.", nameof(profileId));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

                if (currentUser.ProfileId == profileId) throw new ArgumentException($"Current user cannot set admin status to itself.", nameof(profileId));

                return Ok(await _profilesQueryRepository.SetAsAdmin(profileId));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Removes the specified profile identifier as admin.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <exception cref="ArgumentException">Profile is null. {profile}</exception>
        /// <exception cref="ArgumentException">Current user does not have admin status.</exception>
        /// <exception cref="ArgumentException">Current user cannot set admin status to itself. {profile}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveAdmin")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveAdmin([FromBody] string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"Profile is null.", nameof(profileId));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

                if (currentUser.ProfileId == profileId) throw new ArgumentException($"Current user cannot remove admin status from itself.", nameof(profileId));

                return Ok(await _profilesQueryRepository.RemoveAdmin(profileId));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileById/{profileId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Profile>> Get(string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser.ProfileId == profileId) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileId));

                var profile = await _profilesQueryRepository.GetProfileById(profileId);

                if (profile == null)
                {
                    return NotFound();
                }

                // Notify profile that currentUser has visited their profile unless currentUser is admin.
                if (!currentUser.Admin)
                {
                    await _profilesQueryRepository.AddVisitedToProfiles(currentUser, profile);
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Gets profiles by identifiers.</summary>
        /// <param name="profileIds">The profile identifiers.</param>
        /// <exception cref="ArgumentException">ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetProfilesByIds")]
        public async Task<IActionResult> GetProfilesByIds([FromBody] RequestBody requestBody, [FromQuery] ParameterFilter parameterFilter)
        {
            if (requestBody.ProfileIds == null) throw new ArgumentException($"ProfileIds is either null {requestBody.ProfileIds} or length is < 1 {requestBody.ProfileIds.Length}.", nameof(requestBody.ProfileIds));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetProfilesByIds(currentUser, requestBody.ProfileIds, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        /// <summary>Adds the visited to profiles.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <exception cref="ArgumentException">ProfileId is null. {profileId}</exception>
        /// <exception cref="ArgumentException">ProfileId is similar to current user profileId. {profileId}</exception>
        /// <exception cref="ArgumentException">Profile is not found. {profileId}</exception>
        /// <returns> </returns>
        [NoCache]
        [HttpGet("~/AddVisitedToProfiles/{profileId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddVisitedToProfiles(string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                // Do not notify profile if admin has visited.
                if(currentUser.Admin) return NoContent();

                if (currentUser.ProfileId == profileId) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileId));

                var profile = await _profilesQueryRepository.GetProfileById(profileId);

                if (profile == null)
                {
                    throw new ArgumentException($"Profile is not found.", nameof(profileId));
                }

                // Notify profile that currentUser has visited their profile.
                await _profilesQueryRepository.AddVisitedToProfiles(currentUser, profile);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }


        /// <summary>Adds like to profile.</summary>
        /// <param name="profileIds">The profile identifiers.</param>
        /// <exception cref="ArgumentException">ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.</exception>
        /// <exception cref="ArgumentException">ProfileId is similar to current user profileId. {profileId}</exception>
        /// <exception cref="ArgumentException">Profile is not found. {profileId}</exception>
        /// <returns> </returns>
        [NoCache]
        [HttpPost("~/AddLikeToProfiles")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddLikeToProfiles([FromBody] string[] profileIds)
        {
            try
            {
                if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                // Cannot Like profile if currentUser is admin.
                if (currentUser.Admin) return NoContent();

                if(profileIds.Contains(currentUser.ProfileId)) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileIds));

                await _profilesQueryRepository.AddLikeToProfiles(currentUser, profileIds);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Removes like from profile.</summary>
        /// <param name="profileId">The profile identifiers.</param>
        /// <exception cref="ArgumentException">ProfileId is null. {profileId}</exception>
        /// <exception cref="ArgumentException">ProfileId is similar to current user profileId. {profileId}</exception>
        /// <exception cref="ArgumentException">Profile is not found. {profileId}</exception>
        /// <returns> </returns>
        [NoCache]
        [HttpPost("~/RemoveLikeFromProfiles")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveLikeFromProfiles([FromBody] string[] profileIds)
        {
            try
            {
                if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                // Cannot Like profile if currentUser is admin.
                if (currentUser.Admin) return NoContent();

                if (profileIds.Contains(currentUser.ProfileId)) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileIds));

                await _profilesQueryRepository.RemoveLikeFromProfiles(currentUser, profileIds);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Add a complain to profile.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <exception cref="ArgumentException">ProfileId is null. {profileId}</exception>
        /// <exception cref="ArgumentException">ProfileId is similar to current user profileId. {profileId}</exception>
        /// <exception cref="ArgumentException">Profile is not found. {profileId}</exception>
        /// <returns> </returns>
        [NoCache]
        [HttpPost("~/AddComplainToProfile")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddComplainToProfile([FromBody] string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                // An admin cannot complain.
                if (currentUser.Admin) return NoContent();

                if (currentUser.ProfileId == profileId) throw new ArgumentException($"ProfileId is similar to current user profileId.", nameof(profileId));

                var profile = await _profilesQueryRepository.GetProfileById(profileId);

                if (profile == null)
                {
                    throw new ArgumentException($"Profile is not found.", nameof(profileId));
                }

                await _profilesQueryRepository.AddComplainToProfile(currentUser, profile);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the specified profile(s) based on a filter. Eg. { Body: 'something' }
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <exception cref="ArgumentException">ProfileFilter is null. {requestBody.ProfileFilter}</exception>
        /// <exception cref="ArgumentException">Current users profileFilter cannot find any matching profiles. {requestBody.ProfileFilter}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetProfileByFilter")]
        public async Task<IActionResult> GetProfileByFilter([FromBody] RequestBody requestBody, [FromQuery] ParameterFilter parameterFilter)
        {
            if (requestBody.ProfileFilter == null) throw new ArgumentException($"ProfileFilter is null.", nameof(requestBody.ProfileFilter));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetProfileByFilter(currentUser, requestBody.ProfileFilter, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles }); 
        }

        /// <summary>Gets the specified profiles based on the CurrentUser's filter.</summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileByCurrentUsersFilter/")]
        /// <exception cref="ArgumentException">Current users profileFilter is null. {parameterFilter}</exception>
        public async Task<IActionResult> GetProfileByCurrentUsersFilter([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileFilter == null) throw new ArgumentException($"Current users profileFilter is null.", nameof(parameterFilter));

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetProfileByFilter(currentUser, currentUser.ProfileFilter, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        /// <summary>
        /// Gets Latest Profiles OrderBy.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetLatestProfiles/")]
        public async Task<IActionResult> GetLatestProfiles([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetLatestProfiles(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        /// <summary>
        /// Gets Bookmarked Profiles.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetBookmarkedProfiles/")]
        public async Task<IActionResult> GetBookmarkedProfiles([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetBookmarkedProfiles(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        /// <summary>
        /// Gets Profiles who has visited my profile.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfilesWhoVisitedMe/")]
        public async Task<IActionResult> GetProfilesWhoVisitedMe([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetProfilesWhoVisitedMe(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        /// <summary>
        /// Gets Profiles who has bookmarked my profile.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfilesWhoBookmarkedMe/")]
        public async Task<IActionResult> GetProfilesWhoBookmarkedMe([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetProfilesWhoBookmarkedMe(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        /// <summary>
        /// Gets Profiles who likes my profile.
        /// </summary>
        /// <param name="parameterFilter"></param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfilesWhoLikesMe/")]
        public async Task<IActionResult> GetProfilesWhoLikesMe([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _profilesQueryRepository.GetProfilesWhoLikesMe(currentUser, parameterFilter.OrderByType, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.profiles });
        }

        #region Maintenance

        ///// <summary>Deletes 10 old profiles that are more than 30 days since last active.</summary>
        //[NoCache]
        //[HttpDelete("~/DeleteOldProfiles")]
        //[ProducesResponseType((int)HttpStatusCode.NoContent)]
        //public async Task<IActionResult> DeleteOldProfiles(int daysBack, int limit)
        //{
        //    try
        //    {
        //        if(_deleteOldProfiles)
        //        {
        //            var currentUser = await _helper.GetCurrentUserProfile(User);

        //            if (currentUser == null || currentUser.Name == null)
        //            {
        //                return NotFound();
        //            }

        //            if (!currentUser.Admin) throw new ArgumentException($"Current user does not have admin status.");

        //            await _helper.DeleteOldProfiles(daysBack, limit);                    
        //        }

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.ToString());
        //    }
        //}

        #endregion
    }
}
