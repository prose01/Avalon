using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IHelperMethods _helper;

        public ProfilesController(IProfileRepository profileRepository, IHelperMethods helperMethods)
        {
            _profileRepository = profileRepository;
            _helper = helperMethods;
        }

        /// <summary>
        /// Gets the current user profile.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/Profiles/GetCurrentUserProfile/")]
        public async Task<CurrentUser> GetCurrentUserProfile()
        {
            return await _helper.GetCurrentUserProfile(User);
        }

        /// <summary>
        /// Gets all Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public async Task<IEnumerable<CurrentUser>> Get()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profileRepository.GetAllProfiles(currentUser);
        }

        // GET api/profiles/5
        /// <summary>
        /// Gets the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [NoCache]
        [HttpGet("{profileId}")]
        public async Task<CurrentUser> Get(string profileId)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId == profileId) return null;

            return await _profileRepository.GetProfileById(profileId) ?? null;
        }

        /// <summary>
        /// Add new profile to database
        /// </summary>
        /// <param name="profile"> The value.</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CurrentUser item)
        {
            if (!ModelState.IsValid) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User); // New user don't exists!!!

            if (currentUser.ProfileId != item.ProfileId) return BadRequest();

            return Ok(_profileRepository.AddProfile(item));

            //if (ModelState.IsValid)
            //{
            //    //...
            //    return Ok();
            //}
            //return BadRequest();

            //// Tell user the name is not valid!
            //if (string.IsNullOrEmpty(item.Name))
            //{
            //    return new BadRequestObjectResult(ModelState);
            //}

            //var profile = _profileRepository.GetProfileByName(item.Name).Result ?? null;

            //// Tell user that name already exits!
            //if (profile != null)
            //{
            //    return new BadRequestObjectResult(ModelState);
            //}

            //return Ok(_profileRepository.AddProfile(item));
        }

        /// <summary>
        /// Patches the specified profile identifier. Does not work!!!
        /// </summary>
        /// <param name="patch">The patch.</param>
        //[HttpPatch]
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
        [HttpPut]
        public async Task<IActionResult> Put([FromBody]CurrentUser item)
        {
            if (!ModelState.IsValid) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId != item.ProfileId) return BadRequest();

            item._id = currentUser._id; // _id is immutable and the type is unknow by BluePenguin.

            return Ok(_profileRepository.UpdateProfile(item));
        }

        // DELETE api/profiles/23243423
        /// <summary>
        /// Deletes the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        [HttpDelete("{profileId}")]
        public void Delete(string profileId)
        {
            _profileRepository.RemoveProfile(profileId);
        }


        /// <summary>
        /// Gets the specified profile based on a filter. Eg. { Body: 'something' }
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetProfileByFilter/")]
        public async Task<CurrentUser> GetProfileByFilter(string profileFilter)
        {
            return await _profileRepository.GetProfileByFilter(profileFilter) ?? new CurrentUser(); // Should be null if no filter match.
        }

        /// <summary>Adds the profiles to currentUser bookmarks.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/Profiles/AddProfilesToBookmarks")]
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
        [HttpPost("~/Profiles/RemoveProfilesFromBookmarks")]
        public async Task<IActionResult> RemoveProfilesFromBookmarks([FromBody]string[] profileIds)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            var currentUser = await _helper.GetCurrentUserProfile(User);

            return Ok(_profileRepository.RemoveProfilesFromBookmarks(currentUser, profileIds));
        }

    }
}
