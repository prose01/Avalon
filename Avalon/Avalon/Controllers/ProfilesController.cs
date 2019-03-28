﻿using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ILogger<ProfilesController> _logger;
        private readonly string _claimsNickname;
        private readonly string _claimsEmail;

        public ProfilesController(IOptions<Settings> settings, IProfileRepository profileRepository, ILogger<ProfilesController> logger)
        {
            _profileRepository = profileRepository;
            _logger = logger;
            _claimsNickname = settings.Value.ClaimsNickname;
            _claimsEmail = settings.Value.ClaimsEmail;
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public Task<IEnumerable<Profile>> Get()
        {
            var nickname = User.Claims.FirstOrDefault(c => c.Type == _claimsNickname)?.Value;
            var email = User.Claims.FirstOrDefault(c => c.Type == _claimsEmail)?.Value;

            var profileName = string.Empty;

            if (nickname == "peterrose03")
            {
                profileName = "Peter Rose";
            }

            return GetProfileInternal(profileName);
        }

        private async Task<IEnumerable<Profile>> GetProfileInternal(string profileName)
        {
            return await _profileRepository.GetAllProfiles(profileName);
        }


        // GET api/profiles/5
        /// <summary>
        /// Gets the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <returns></returns>
        [HttpGet("{profileId}")]
        public Task<Profile> Get(string profileId)
        {
            return GetProfileByIdInternal(profileId);
        }

        private async Task<Profile> GetProfileByIdInternal(string profileId)
        {
            return await _profileRepository.GetProfile(profileId) ?? new Profile();
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
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="patch">The patch.</param>
        [HttpPatch("{profileId}")]
        public IActionResult Patch(string profileId, [FromBody]JsonPatchDocument<Profile> patch)
        {
            var item = _profileRepository.GetProfile(profileId).Result ?? null;

            patch.ApplyTo(item, ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            return Ok(_profileRepository.UpdateProfile(profileId, item));
        }


        // Put api/profiles/5       TODO: "SLET denne metode når Patch virker"
        /// <summary>
        /// Update the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="put">The value.</param>
        [HttpPut("{profileId}")]
        public IActionResult Put(string profileId, [FromBody]Profile item)
        {
            var profile = _profileRepository.GetProfile(profileId).Result ?? null;

            if (profile == null)
            {
                return new BadRequestObjectResult(ModelState);
            }

            profile.Name = item.Name;
            profile.Body = item.Body;

            return Ok(_profileRepository.UpdateProfile(profileId, profile));
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
        [HttpGet("~/api/GetProfileByFilter/")]
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
