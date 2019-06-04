﻿using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalon.Helpers
{
    public class HelperMethods : IHelperMethods
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ILogger<HelperMethods> _logger;
        private readonly string _claimsEmail;

        public HelperMethods(IOptions<Settings> settings, IProfileRepository profileRepository, ILogger<HelperMethods> logger)
        {
            _profileRepository = profileRepository;
            _logger = logger;
            _claimsEmail = settings.Value.ClaimsEmail;
        }

        /// <summary>
        /// Gets the current user profile.
        /// </summary>
        /// <returns></returns>
        public async Task<Profile> GetCurrentUserProfile(ClaimsPrincipal user)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == _claimsEmail)?.Value;

            return await _profileRepository.GetCurrentProfileByEmail(email) ?? new Profile();
        }
    }
}