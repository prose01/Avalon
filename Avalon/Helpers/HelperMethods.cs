using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Avalon.Helpers
{
    public class HelperMethods : IHelperMethods
    {
        private readonly ICurrentUserRepository _profileRepository;
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly string _nameidentifier;
        private readonly string _auth0ApiIdentifier;
        private readonly string _auth0TokenAddress;

        public HelperMethods(IOptions<Settings> settings, ICurrentUserRepository profileRepository, IProfilesQueryRepository profilesQueryRepository)
        {
            _nameidentifier = settings.Value.Auth0Id;
            _auth0ApiIdentifier = settings.Value.Auth0ApiIdentifier;
            _auth0TokenAddress = settings.Value.Auth0TokenAddress;
            _profileRepository = profileRepository;
            _profilesQueryRepository = profilesQueryRepository;
        }

        /// <summary>Gets the current user profile.</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentUserProfile(ClaimsPrincipal user)
        {
            try
            {
                var auth0Id = user.Claims.FirstOrDefault(c => c.Type == _nameidentifier)?.Value;

                return await _profileRepository.GetCurrentProfileByAuth0Id(auth0Id) ?? new CurrentUser(); // TODO: Burde smide en fejl hvis bruger ikke findes.
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the current auth0 identifier for the user.</summary>
        /// <param name="user">The user.</param>
        /// <returns>Auth0Id</returns>
        public string GetCurrentUserAuth0Id(ClaimsPrincipal user)
        {
            try
            {
                return user.Claims.FirstOrDefault(c => c.Type == _nameidentifier)?.Value;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Deletes the profile from Auth0. There is no going back!</summary>
        /// <param name="profileId">The profile identifier.</param>
        public async Task DeleteProfileFromAuth0(string profileId)
        {
            try
            {
                var profile = await _profilesQueryRepository.GetProfileById(profileId);

                if (profile == null) return;

                var token = GetAuth0Token();

                var client = new RestClient(_auth0ApiIdentifier + "users/" + profile.Auth0Id);
                client.ThrowOnAnyError = true;
                var request = new RestRequest(Method.DELETE);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + token);
                IRestResponse response = await client.ExecuteAsync(request, CancellationToken.None);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the auth0 token.</summary>
        /// <returns></returns>
        private string GetAuth0Token()
        {
            try
            {
                var client = new RestClient(_auth0TokenAddress);
                client.ThrowOnAnyError = true;
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", "{\"client_id\":\"ZKHIqFGxgm9OBc5Bxn1226pT9kXHLknW\",\"client_secret\":\"OpIPkn9Y4Ctoh9UuUbUpGzHybTNVEFevel0yQneY6X5ITKyaRwJEMbHYB3mNofkN\",\"audience\":\"https://bluepenguin.eu.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                return JsonSerializer.Deserialize<AccessToken>(response.Content).access_token;
            }
            catch
            {
                throw;
            }
        }
    }
}
