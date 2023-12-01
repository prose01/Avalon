using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
        private readonly string _auth0_Client_id;
        private readonly string _auth0_Client_secret;
        private readonly int _millisecondsAbsoluteExpiration = 28800000;
        private IMemoryCache _cache;

        public HelperMethods(IConfiguration config, ICurrentUserRepository profileRepository, IProfilesQueryRepository profilesQueryRepository)
        {
            _nameidentifier = config.GetValue<string>("Auth0_Claims_nameidentifier"); 
            _auth0ApiIdentifier = config.GetValue<string>("Auth0_ApiIdentifier");
            _auth0TokenAddress = config.GetValue<string>("Auth0_TokenAddress");
            _auth0_Client_id = config.GetValue<string>("Auth0_Client_id");
            _auth0_Client_secret = config.GetValue<string>("Auth0_Client_secret");
            _profileRepository = profileRepository;
            _profilesQueryRepository = profilesQueryRepository;
            _millisecondsAbsoluteExpiration = config.GetValue<int>("MillisecondsAbsoluteExpirationCache"); 
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        /// <summary>Gets the current user profile.</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<CurrentUser> GetCurrentUserProfile(ClaimsPrincipal user)
        {
            try
            {
                var auth0Id = user.Claims.FirstOrDefault(c => c.Type == _nameidentifier)?.Value;

                return await _profileRepository.GetCurrentProfileByAuth0Id(auth0Id);
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
                var auth0Id = await _profilesQueryRepository.GetAuth0Id(profileId);

                if (string.IsNullOrEmpty(auth0Id)) return;

                _ = _cache.TryGetValue("Auth0Token", out string token);

                var accessToken = string.IsNullOrEmpty(token) ? await GetAuth0Token() : token;

                var url = _auth0ApiIdentifier + "users/" + auth0Id;
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Delete);
                request.AddHeader("authorization", "Bearer " + accessToken);
                var response = await client.ExecuteAsync(request, CancellationToken.None);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the auth0 token.</summary>
        /// <returns></returns>
        private async Task<string> GetAuth0Token()
        {
            try
            {
                // Save token until it expires, to minimize the number of tokens requested. Auth0 customers are billed based on the number of Machine to Machine Access Tokens issued by Auth0. 
                var client = new RestClient(_auth0TokenAddress);
                var request = new RestRequest(_auth0TokenAddress, Method.Post);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", $"{{\"client_id\":\"{_auth0_Client_id}\",\"client_secret\":\"{_auth0_Client_secret}\",\"audience\":\"{_auth0ApiIdentifier}\",\"grant_type\":\"client_credentials\"}}", ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                var token = JsonSerializer.Deserialize<AccessToken>(response.Content).access_token;

                MemoryCacheEntryOptions options = new()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_millisecondsAbsoluteExpiration)
                };

                _cache.Set("Auth0Token", token, options);

                return token;
            }
            catch
            {
                throw;
            }
        }
    }
}
