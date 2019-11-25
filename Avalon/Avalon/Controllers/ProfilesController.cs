﻿using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
        public async Task<Profile> GetCurrentUserProfile()
        {
            return await _helper.GetCurrentUserProfile(User);
        }

        /// <summary>
        /// Gets all Profiles.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public async Task<IEnumerable<Profile>> Get()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await GetProfileInternal(currentUser);
        }

        private async Task<IEnumerable<Profile>> GetProfileInternal(Profile currentUser)
        {
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
        public async Task<Profile> Get(string profileId)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId == profileId) return null;

            return await GetProfileByIdInternal(profileId);
        }

        private async Task<Profile> GetProfileByIdInternal(string profileId)
        {
            return await _profileRepository.GetProfile(profileId) ?? null;
        }

        // POST api/profiles
        /// <summary>
        /// Posts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        [HttpPost]
        public IActionResult Post([FromBody]Profile item)
        {
            // Tell user the name is not valid!
            if (string.IsNullOrEmpty(item.Name))
            {
                return new BadRequestObjectResult(ModelState);
            }

            var profile = _profileRepository.GetProfileByName(item.Name).Result ?? null;

            // Tell user that name already exits!
            if (profile != null)
            {
                return new BadRequestObjectResult(ModelState);
            }

            return Ok(_profileRepository.AddProfile(item));
        }


        // Patch api/profiles/5
        /// <summary>
        /// Patches the specified profile identifier.
        /// </summary>
        /// <param name="patch">The patch.</param>
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<Profile> patch)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            var item = _profileRepository.GetProfile(currentUser.ProfileId).Result ?? null;

            patch.ApplyTo(item, ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            return Ok(_profileRepository.UpdateProfile(item));
        }


        // Put api/profiles/5       TODO: "SLET denne metode når Patch virker"
        /// <summary>Update the specified profile identifier.</summary>
        /// <param name="item"></param>
        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody]Profile item)
        {
            if (item == null) return new BadRequestObjectResult(nameof(item));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.ProfileId != currentUser.ProfileId) return new BadRequestObjectResult(nameof(item));

            //var profile = _profileRepository.GetProfile(profileId).Result ?? null;

            //if (profile == null) return new BadRequestObjectResult(ModelState);

            currentUser.Name = item.Name;
            currentUser.Body = item.Body;

            return Ok(_profileRepository.UpdateProfile(currentUser));
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
        public Task<Profile> GetProfileByFilter(string profileFilter)
        {
            return GetProfileByFilterInternal(profileFilter);
        }

        private async Task<Profile> GetProfileByFilterInternal(string profileFilter)
        {
            return await _profileRepository.GetProfileByFilter(profileFilter) ?? new Profile();
        }
    }
}
