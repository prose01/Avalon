using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.JsonPatch;
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
    public class CurrentUserController : Controller
    {
        private readonly ICurrentUserRepository _profileRepository;
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly IHelperMethods _helper;

        public CurrentUserController(ICurrentUserRepository profileRepository, IProfilesQueryRepository profilesQueryRepository, IHelperMethods helperMethods)
        {
            _profileRepository = profileRepository;
            _profilesQueryRepository = profilesQueryRepository;
            _helper = helperMethods;
        }

        /// <summary>
        /// Gets the current user profile.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/CurrentUser")]
        public async Task<CurrentUser> GetCurrentUserProfile()
        {
            return await _helper.GetCurrentUserProfile(User);
        }

        /// <summary>
        /// Add new profile to database
        /// </summary>
        /// <param name="profile"> The value.</param>
        [HttpPost("~/CurrentUser")]
        public async Task<IActionResult> Post([FromBody] CurrentUser item)
        {
            try
            {
                // Check if Auth0Id exists
                var auth0Id = _helper.GetCurrentUserAuth0Id(User);

                if (string.IsNullOrEmpty(auth0Id)) return BadRequest();

                // Check if Name already exists.
                if (_profilesQueryRepository.GetProfileByName(item.Name).Result != null) return BadRequest();

                // Check if Auth0Id already exists.
                if (_profilesQueryRepository.GetProfileByAuth0Id(auth0Id).Result != null) return BadRequest();

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
                item.ProfileFilter = this.CreateBasicProfileFilter(item);

                return Ok(_profileRepository.AddProfile(item));
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

        //    var item = _profileRepository.GetProfileById(currentUser.ProfileId).Result ?? null;

        //    patch.ApplyTo(item, ModelState);

        //    if (!ModelState.IsValid)
        //    {
        //        return new BadRequestObjectResult(ModelState);
        //    }

        //    return Ok(_profileRepository.UpdateProfile(item));
        //}


        /// TODO: "SLET denne metode når Patch virker"
        /// <summary>Update the specified profile identifier.</summary>
        /// <param name="item">The profile</param>
        [NoCache]
        [HttpPut("~/CurrentUser")]
        public async Task<IActionResult> Put([FromBody] CurrentUser item)
        {
            //if (!ModelState.IsValid) return BadRequest(); unnecessary

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser.ProfileId != item.ProfileId) return BadRequest();

                // Certain properties cannot be changed by the user.
                item._id = currentUser._id; // _id is immutable and the type is unknow by BluePenguin.
                item.Admin = currentUser.Admin; // No user is allowed to set themselves as Admin!
                item.Name = currentUser.Name; // You cannot change your name after create.
                item.Bookmarks = currentUser.Bookmarks;
                item.ChatMemberslist = currentUser.ChatMemberslist;
                item.IsBookmarked = currentUser.IsBookmarked;
                item.Visited = currentUser.Visited;
                item.ProfileFilter = currentUser.ProfileFilter;
                item.Images = currentUser.Images;
                item.CreatedOn = currentUser.CreatedOn;

                // Update ProfileFilter Gender when CurrentUser is updated.
                if(currentUser.SexualOrientation != item.SexualOrientation || currentUser.Gender != item.Gender)
                {
                    item.ProfileFilter.Gender = this.CreateBasicProfileFilter(item).Gender;
                }

                return Ok(_profileRepository.UpdateProfile(item));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the CurrentUser profile.
        /// </summary>
        [HttpDelete("~/CurrentUser")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.Admin) return BadRequest(); // Admins cannot delete themseleves.

            try
            {
                await _helper.DeleteProfileFromAuth0(currentUser.ProfileId);
                await _profileRepository.DeleteCurrentUser(currentUser.ProfileId);
                //await _helper.DeleteProfileFromArtemis(currentUser.ProfileId);        // TODO: Delete all photos for Profile in Artemis

                return Ok();
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
        public async Task<IActionResult> SaveProfileFilter([FromBody]ProfileFilter profileFilter)
        {
            if (profileFilter == null) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                return Ok(_profileRepository.SaveProfileFilter(currentUser, profileFilter));
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
        public async Task<ProfileFilter> LoadProfileFilter()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profileRepository.LoadProfileFilter(currentUser);
        }

        /// <summary>Adds the profiles to currentUser bookmarks and ChatMemberslist.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/AddProfilesToBookmarks")]
        public async Task<IActionResult> AddProfilesToBookmarks([FromBody] string[] profileIds)
        {
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                await _profileRepository.AddProfilesToBookmarks(currentUser, profileIds);
                await _profileRepository.AddProfilesToChatMemberslist(currentUser, profileIds);

                // Notify other profiles that currentUser has bookmarked their profile.
                await _profilesQueryRepository.AddIsBookmarkedToProfiles(currentUser, profileIds);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }


        /// <summary>Removes the profiles from currentUser bookmarks and ChatMemberslist.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveProfilesFromBookmarks")]
        public async Task<IActionResult> RemoveProfilesFromBookmarks([FromBody] string[] profileIds)
        {
            if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                await _profileRepository.RemoveProfilesFromBookmarks(currentUser, profileIds);
                await _profileRepository.RemoveProfilesFromChatMemberslist(currentUser, profileIds);

                return Ok();
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
        public async Task<IActionResult> BlockChatMembers([FromBody] string[] profileIds)
        {
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                await _profileRepository.BlockChatMembers(currentUser, profileIds);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        #region Helper methods

        /// <summary>Creates the basic profile filter.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        private ProfileFilter CreateBasicProfileFilter(CurrentUser currentUser)
        {
            var filter = new ProfileFilter();

            switch (currentUser.SexualOrientation)
            {
                case SexualOrientationType.Heterosexual:
                    filter.Gender = currentUser.Gender == GenderType.Male ? GenderType.Female : GenderType.Male;
                    break;
                case SexualOrientationType.Homosexual:
                    filter.Gender = currentUser.Gender;
                    break;
                case SexualOrientationType.Bisexual:
                    break;
                case SexualOrientationType.Asexual:
                    break;
                default:
                    filter.Gender = currentUser.Gender == GenderType.Male ? GenderType.Female : GenderType.Male;
                    break;
            }

            return filter;
        }

        #endregion
    }
}
