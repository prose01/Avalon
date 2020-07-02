using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalon.Helpers
{
    public class HelperMethods : IHelperMethods
    {
        private readonly ICurrentUserRepository _profileRepository;
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly string _nameidentifier;
        private readonly string _auth0ApiIdentifier;
        private readonly IHttpClientFactory _clientFactory;
        private readonly HttpClient _client;

        public HelperMethods(IOptions<Settings> settings, IHttpClientFactory clientFactory, ICurrentUserRepository profileRepository, IProfilesQueryRepository profilesQueryRepository)
        {
            _nameidentifier = settings.Value.Auth0Id;
            _auth0ApiIdentifier = settings.Value.Auth0ApiIdentifier;
            _clientFactory = clientFactory;
            _profileRepository = profileRepository;
            _profilesQueryRepository = profilesQueryRepository;

            _client = _clientFactory.CreateClient();
        }

        /// <summary>Gets the current user profile.</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentUserProfile(ClaimsPrincipal user)
        {
            var auth0Id = user.Claims.FirstOrDefault(c => c.Type == _nameidentifier)?.Value;

            return await _profileRepository.GetCurrentProfileByAuth0Id(auth0Id) ?? new CurrentUser(); // TODO: Burde smide en fejl hvis bruger ikke findes.
        }

        /// <summary>Gets the current auth0 identifier for the user.</summary>
        /// <param name="user">The user.</param>
        /// <returns>Auth0Id</returns>
        public string GetCurrentUserAuth0Id(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == _nameidentifier)?.Value;
        }

        public async Task<IEnumerable<string>> DeleteProfile(string profileId)
        {
            try
            {
                var profile = await _profilesQueryRepository.GetProfileById(profileId);

                var response = await _client.DeleteAsync(_auth0ApiIdentifier + "User/" + profile.Auth0Id);  // https://bluepenguin.eu.auth0.com/api/v2/User/auth0|5ef5f71683dfba001446bbe4

                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync
                    <IEnumerable<string>>(responseStream);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
