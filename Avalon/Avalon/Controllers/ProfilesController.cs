using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ProfilesController : Controller
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ILogger<ProfilesController> _logger;

        public ProfilesController(IProfileRepository profileRepository, ILogger<ProfilesController> logger)
        {
            _profileRepository = profileRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet]
        public Task<IEnumerable<Profile>> Get()
        {
            _logger.LogInformation("NLog says hello");
            return GetProfileInternal();
        }

        private async Task<IEnumerable<Profile>> GetProfileInternal()
        {
            return await _profileRepository.GetAllProfiles();
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
    }
}
