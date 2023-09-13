using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class CurrentUserController : Controller
    {
        private readonly ICurrentUserRepository _currentUserRepository;
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IHelperMethods _helper;

        public CurrentUserController(ICurrentUserRepository currentUserRepository, IProfilesQueryRepository profilesQueryRepository, IGroupRepository groupRepository, IHelperMethods helperMethods)
        {
            _currentUserRepository = currentUserRepository;
            _profilesQueryRepository = profilesQueryRepository;
            _groupRepository = groupRepository;
            _helper = helperMethods;
        }

        /// <summary>
        /// Gets the current user profile.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/CurrentUser")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                return Ok(currentUser);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Add new profile to database
        /// </summary>
        /// <param name="profile"> The value.</param>
        [HttpPost("~/CurrentUser")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromBody] CurrentUser item)
        {
            try
            {
                // Check if Auth0Id exists
                var auth0Id = _helper.GetCurrentUserAuth0Id(User);

                if (string.IsNullOrEmpty(auth0Id)) return BadRequest();

                // Check if Auth0Id already exists.
                if (_profilesQueryRepository.GetProfileByAuth0Id(auth0Id).Result != null) return BadRequest("User already exists.");

                // Set admin default to false! Only other admins can give this privilege.
                item.Admin = false;

                item.Auth0Id = auth0Id;

                // Initiate empty lists and other defaults
                item.Tags ??= new List<string>();
                item.Bookmarks = new List<string>();
                item.ChatMemberslist = new List<ChatMember>();
                item.Images = new List<ImageModel>();
                item.IsBookmarked = new Dictionary<string, DateTime>();
                item.Visited = new Dictionary<string, DateTime>();
                item.Likes = new List<string>();
                item.Complains = new Dictionary<string, DateTime>();
                item.Groups = new List<string>();

                await _currentUserRepository.AddProfile(item);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Patches the specified profile identifier. Does not work!!!      https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.1
        /// </summary>
        /// <param name="patch">The patch.</param>
        //[HttpPatch("~/CurrentUser/")]
        //public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<CurrentUser> patch)
        //{
        //    var currentUser = await _helper.GetCurrentUserProfile(User);

        //    var item = _currentUserRepository.GetProfileById(currentUser.ProfileId).Result ?? null;

        //    patch.ApplyTo(item, ModelState);

        //    if (!ModelState.IsValid)
        //    {
        //        return new BadRequestObjectResult(ModelState);
        //    }

        //    return Ok(_currentUserRepository.UpdateProfile(item));
        //}


        /// NOTE: "Delete this method when Patch works"
        /// <summary>Update the specified profile identifier.</summary>
        /// <param name="item">The profile</param>
        [NoCache]
        [HttpPut("~/CurrentUser")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Put([FromBody] CurrentUser item)
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                if (currentUser.ProfileId != item.ProfileId) return BadRequest();

                // Certain properties cannot be changed by the user.
                item._id = currentUser._id; // _id is immutable and the type is unknow by BluePenguin.
                item.Auth0Id = currentUser.Auth0Id; // No user is allowed to see Auth0Id and it is therefore unknow to BluePenguin.
                item.Admin = currentUser.Admin; // No user is allowed to set themselves as Admin!
                item.Name = currentUser.Name; // You cannot change your name after create.
                item.ProfileFilter = currentUser.ProfileFilter;
                item.Images = currentUser.Images;
                item.CreatedOn = currentUser.CreatedOn;
                item.Complains = currentUser.Complains; // You cannot run away from your complains

                // If currentUser has changed country reset all connections to other profiles.
                if (item.Countrycode != currentUser.Countrycode)
                {
                    item.Bookmarks = new List<string>();
                    item.ChatMemberslist = new List<ChatMember>();
                    item.IsBookmarked = new Dictionary<string, DateTime>();
                    item.Visited = new Dictionary<string, DateTime>();
                    item.Likes = new List<string>();
                    item.Groups = new List<string>();
                }
                else
                {
                    item.Bookmarks = currentUser.Bookmarks;
                    item.ChatMemberslist = currentUser.ChatMemberslist;
                    item.IsBookmarked = currentUser.IsBookmarked;
                    item.Visited = currentUser.Visited;
                    item.Likes = currentUser.Likes;
                    item.Groups = currentUser.Groups;
                }

                await _currentUserRepository.UpdateProfile(item);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the CurrentUser profile.
        /// </summary>
        /// <exception cref="ArgumentException">Admins cannot delete themselves.</exception>
        [HttpDelete("~/CurrentUser")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || currentUser.Name == null)
            {
                return NotFound();
            }

            if (currentUser.Admin) throw new ArgumentException($"Admins cannot delete themselves.");

            try
            {
                await _helper.DeleteProfileFromAuth0(currentUser.ProfileId);
                await _currentUserRepository.DeleteCurrentUser(currentUser.ProfileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Saves the profile filter to currentUser.</summary>
        /// <param name="profileFilter">The profile filter.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/SaveProfileFilter")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SaveProfileFilter([FromBody] ProfileFilter profileFilter)
        {
            if (profileFilter == null) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                await _currentUserRepository.SaveProfileFilter(currentUser, profileFilter);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Load the profile filter from currentUser.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/LoadProfileFilter")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ProfileFilter>> LoadProfileFilter()
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                return await _currentUserRepository.LoadProfileFilter(currentUser);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Adds the profiles to currentUser bookmarks and ChatMemberslist.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/AddProfilesToBookmarks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddProfilesToBookmarks([FromBody] string[] profileIds)
        {
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                await _currentUserRepository.AddProfilesToBookmarks(currentUser, profileIds);
                await _currentUserRepository.AddProfilesToChatMemberslist(currentUser, profileIds);

                // Notify other profiles that currentUser has bookmarked their profile, unless currentUser is admin.
                if (!currentUser.Admin)
                {
                    await _profilesQueryRepository.AddIsBookmarkedToProfiles(currentUser, profileIds);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Removes the profiles from currentUser bookmarks and ChatMemberslist.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <exception cref="ArgumentException">ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}. {profileIds}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveProfilesFromBookmarks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveProfilesFromBookmarks([FromBody] string[] profileIds)
        {
            if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                await _currentUserRepository.RemoveProfilesFromBookmarks(currentUser, profileIds);
                await _currentUserRepository.RemoveProfilesFromChatMemberslist(currentUser, profileIds);

                /// Remove currentUser.profileId from IsBookmarked list of every profile in profileIds list.
                await _profilesQueryRepository.RemoveIsBookmarkedFromProfiles(currentUser, profileIds);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Blocks or unblocks chatMembers.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/BlockChatMembers")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> BlockChatMembers([FromBody] string[] profileIds)
        {
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                // Blocks or unblocks chatMembers if not admin.
                if (!currentUser.Admin)
                {
                    await _currentUserRepository.BlockChatMembers(currentUser, profileIds);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        #region Groups

        /// <summary>Get all groups with same countrycode as currentUser.</summary>
        /// <returns>Returns list of groups.</returns>
        [NoCache]
        [HttpGet("~/GetGroups")]
        public async Task<IActionResult> GetGroups([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || currentUser.Name == null)
            {
                int total = 0;
                List<GroupModel> groups = new List<GroupModel> { };

                return Json(new { total, groups });
            }

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _groupRepository.GetGroups(currentUser, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.groups });
        }

        /// <summary>Get Groups that CurrentUser is member of.</summary>
        /// <returns>Returns list of groups.</returns>
        [NoCache]
        [HttpGet("~/GetCurrenUsersGroups")]
        public async Task<IEnumerable<GroupModel>> GetCurrenUsersGroups([FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || currentUser.Name == null)
            {
                List<GroupModel> emptyGroups = new List<GroupModel> { };
                return emptyGroups;
            }

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var limit = parameterFilter.PageSize > currentUser?.Groups.Count ? currentUser.Groups.Count : parameterFilter.PageSize;

            var groups = currentUser.Groups.Skip(skip).Take(limit);

            return await _groupRepository.GetGroupsByIds(groups.ToArray());
        }

        /// <summary>
        /// Gets the specified groups based on a filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>Returns list of groups.</returns>
        [NoCache]
        [HttpPost("~/GetGroupsByFilter")]
        public async Task<IActionResult> GetGroupsByFilter([FromBody] string filter, [FromQuery] ParameterFilter parameterFilter)
        {
            if (string.IsNullOrEmpty(filter) || filter == "null")
                return await this.GetGroups(parameterFilter);

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || currentUser.Name == null)
            {
                int total = 0;
                List<GroupModel> groups = new List<GroupModel> { };

                return Json(new { total, groups });
            }

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var tuple = await _groupRepository.GetGroupsByFilter(currentUser, filter, skip, parameterFilter.PageSize);

            return Json(new { tuple.total, tuple.groups });
        }

        ///// <summary>Remove CurrentUser from groups.</summary>
        ///// <param name="groupIds">The group ids.</param>
        ///// <exception cref="ArgumentException">GroupIds is either null {groupIds} or length is < 1 {groupIds.Length}. {groupIds}</exception>
        ///// <returns></returns>
        //[NoCache]
        //[HttpPost("~/RemoveCurrentUserFromGroups")]
        //[ProducesResponseType((int)HttpStatusCode.NoContent)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //public async Task<IActionResult> RemoveCurrentUserFromGroups(string[] groupIds)
        //{
        //    try
        //    {
        //        if (groupIds == null || groupIds.Length < 1) throw new ArgumentException($"GroupIds is either null {groupIds} or length is < 1 {groupIds.Length}.", nameof(groupIds));

        //        var currentUser = await _helper.GetCurrentUserProfile(User);

        //        if (currentUser == null || currentUser.Name == null)
        //        {
        //            return NotFound();
        //        }

        //        await _groupRepository.RemoveCurrentUserFromGroups(currentUser.ProfileId, groupIds);

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.ToString());
        //    }
        //}

        ///// <summary>Remove groups from CurrentUser.</summary>
        ///// <param name="groupIds">The group ids.</param>
        ///// <exception cref="ArgumentException">GroupIds is either null {groupIds} or length is < 1 {groupIds.Length}. {groupIds}</exception>
        ///// <returns></returns>
        //[NoCache]
        //[HttpPost("~/RemoveGroupsFromCurrentUser")]
        //[ProducesResponseType((int)HttpStatusCode.NoContent)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //public async Task<IActionResult> RemoveGroupsFromCurrentUser(string[] groupIds)
        //{
        //    if (groupIds == null || groupIds.Length < 1) throw new ArgumentException($"GroupIds is either null {groupIds} or length is < 1 {groupIds.Length}.", nameof(groupIds));

        //    try
        //    {
        //        var currentUser = await _helper.GetCurrentUserProfile(User);

        //        if (currentUser == null || currentUser.Name == null)
        //        {
        //            return NotFound();
        //        }

        //        await _currentUserRepository.RemoveGroupsFromCurrentUser(currentUser, groupIds);

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.ToString());
        //    }
        //}

        /// <summary>Create chat group.</summary>
        /// <param name="group">The group.</param>
        /// <exception cref="ArgumentException">Group name is either null or empty. {group}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/CreateGroup")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateGroup([FromBody] GroupModel group)
        {
            if (group.Name.IsNullOrEmpty()) throw new ArgumentException($"Group name is either null or empty.", nameof(group));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                group.GroupId = Guid.NewGuid().ToString();
                group.Countrycode = currentUser.Countrycode;
                group.GroupMemberslist = new List<GroupMember>
                {
                    new GroupMember()
                    {
                        ProfileId = currentUser.ProfileId,
                        Name = currentUser.Name,
                        Blocked = false,
                        Complains = new Dictionary<string, DateTime>()
                    }
                };

                await _currentUserRepository.AddGroupToCurrentUser(currentUser, group.GroupId);
                await _groupRepository.CreateGroup(group);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Join chat group.</summary>
        /// <param name="groupId">The group id.</param>
        /// <exception cref="ArgumentException">GroupId is either null or empty. {groupId}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/JoinGroup")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> JoinGroup([FromBody] string groupId)
        {
            if (groupId.IsNullOrEmpty()) throw new ArgumentException($"GroupId is either null or empty.", nameof(groupId));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                await _currentUserRepository.AddGroupToCurrentUser(currentUser, groupId);
                await _groupRepository.AddCurrentUserToGroup(currentUser, groupId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Remove groups from CurrentUser and CurrentUser from groups.</summary>
        /// <param name="groupIds">The group ids.</param>
        /// <exception cref="ArgumentException">GroupIds is either null {groupIds} or length is < 1 {groupIds.Length}. {groupIds}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveGroupsFromCurrentUserAndCurrentUserFromGroups")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveGroupsFromCurrentUserAndCurrentUserFromGroups(string[] groupIds)
        {
            if (groupIds == null || groupIds.Length < 1) throw new ArgumentException($"GroupIds is either null {groupIds} or length is < 1 {groupIds.Length}.", nameof(groupIds));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                await _currentUserRepository.RemoveGroupsFromCurrentUser(currentUser, groupIds);
                await _groupRepository.RemoveUserFromGroups(currentUser.ProfileId, groupIds);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Add complain to groupMember for group.</summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <exception cref="ArgumentException">GroupId is either null or empty. {groupId}</exception>
        /// <exception cref="ArgumentException">ProfileId is either null or empty. {profileId}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/AddComplainToGroupMember/{groupId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddComplainToGroupMember([FromRoute] string groupId, [FromBody] string profileId)
        {
            if (groupId.IsNullOrEmpty()) throw new ArgumentException($"GroupId is either null or empty.", nameof(groupId));
            if (profileId.IsNullOrEmpty()) throw new ArgumentException($"ProfileId is either null or empty.", nameof(profileId));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || currentUser.Name == null)
                {
                    return NotFound();
                }

                // An admin cannot complain.
                if (currentUser.Admin) return NoContent();

                await _groupRepository.AddComplainToGroupMember(currentUser, groupId, profileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        #endregion


        /// <summary>
        /// Clean CurrentUser for obsolete profile info.
        /// </summary>
        [NoCache]
        [HttpGet("~/CleanCurrentUser")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CleanCurrentUser()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || currentUser.Name == null)
            {
                return NotFound();
            }

            try
            {
                // Clean obsolete profile info from CurrentUser, unless currentUser is admin.
                if (!currentUser.Admin)
                {
                    await _currentUserRepository.CleanCurrentUser(currentUser);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Check if CurrentUser has too many complains.
        /// </summary>
        /// <returns>Returns true if user should get a warning.</returns>
        [NoCache]
        [HttpGet("~/CheckForComplains")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<bool>> CheckForComplains()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || currentUser.Name == null)
            {
                return NotFound();
            }

            try
            {
                // Check if CurrentUser has too many complains, unless currentUser is admin.
                if (!currentUser.Admin)
                {
                    return Ok(await _currentUserRepository.CheckForComplains(currentUser));
                }

                return Ok(false);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
    }
}
