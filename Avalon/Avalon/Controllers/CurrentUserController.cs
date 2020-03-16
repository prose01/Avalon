using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Post([FromBody]CurrentUser item)
        {
            if (!ModelState.IsValid) return BadRequest();

            // Check if Auth0Id exists
            var auth0Id = _helper.GetCurrentUserAuth0Id(User);

            if (string.IsNullOrEmpty(auth0Id)) return BadRequest();

            // Check if Name already exists.
            if (_profilesQueryRepository.GetProfileByName(item.Name).Result != null) return BadRequest();

            // Check if auth0Id already exists.
            if (_profilesQueryRepository.GetProfileByAuth0Id(auth0Id).Result != null) return BadRequest();

            // Set admin default to false! Only other admins can give this privilege.
            item.Admin = false;

            item.Auth0Id = auth0Id;


            return Ok(_profileRepository.AddProfile(item));
        }

        /// <summary>
        /// Patches the specified profile identifier. Does not work!!!
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
        public async Task<IActionResult> Put([FromBody]CurrentUser item)
        {
            if (!ModelState.IsValid) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId != item.ProfileId) return BadRequest();

            item._id = currentUser._id; // _id is immutable and the type is unknow by BluePenguin.

            item.Admin = currentUser.Admin; // No user is allowed to set themselves as Admin!

            return Ok(_profileRepository.UpdateProfile(item));
        }

        /// <summary>
        /// Deletes the CurrentUser profile.
        /// </summary>
        [HttpDelete("~/CurrentUser")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if(currentUser.Admin) return BadRequest(); // Amins cannot delete themseleves.

            // Delete from Auth0

            return Ok(_profileRepository.DeleteCurrentUser(currentUser.ProfileId));
        }

        /// <summary>Adds the profiles to currentUser bookmarks.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/AddProfilesToBookmarks")]
        public async Task<IActionResult> AddProfilesToBookmarks([FromBody]string[] profileIds)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            return Ok(_profileRepository.AddProfilesToBookmarks(currentUser, profileIds));
        }


        /// <summary>Removes the profiles from currentUser bookmarks.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveProfilesFromBookmarks")]
        public async Task<IActionResult> RemoveProfilesFromBookmarks([FromBody]string[] profileIds)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            return Ok(_profileRepository.RemoveProfilesFromBookmarks(currentUser, profileIds));
        }

    }
}
