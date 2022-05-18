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
        private readonly string _auth0_Client_id;
        private readonly string _auth0_Client_secret;

        public HelperMethods(IOptions<Settings> settings, ICurrentUserRepository profileRepository, IProfilesQueryRepository profilesQueryRepository)
        {
            _nameidentifier = settings.Value.Auth0Id;
            _auth0ApiIdentifier = settings.Value.Auth0ApiIdentifier;
            _auth0TokenAddress = settings.Value.Auth0TokenAddress;
            _auth0_Client_id = settings.Value.Client_id;
            _auth0_Client_secret = settings.Value.Client_secret;
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
        public async Task DeleteProfileFromAuth0(string profileId)      // TODO: Check that this still works after upgrading to RestSharp v107 https://restsharp.dev/v107/#restsharp-v107
        {
            try
            {
                var profile = await _profilesQueryRepository.GetProfileById(profileId);

                if (profile == null) return;

                var token = GetAuth0Token();


                var options = new RestClientOptions(_auth0ApiIdentifier + "users/" + profile.Auth0Id)
                {
                    ThrowOnAnyError = true,
                    Timeout = 1000
                };
                var client = new RestClient(options);
                var request = new RestRequest();
                //request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + token);
                var response = await client.ExecuteAsync(request, CancellationToken.None);


                //var client = new RestClient(_auth0ApiIdentifier + "users/" + profile.Auth0Id);
                //client.ThrowOnAnyError = true;
                //var request = new RestRequest(Method.DELETE);
                //request.AddHeader("content-type", "application/json");
                //request.AddHeader("authorization", "Bearer " + token);
                //IRestResponse response = await client.ExecuteAsync(request, CancellationToken.None);
            }
            catch
            {
                throw;
            }
        }

        // TODO: Auth0 customers are billed based on the number of Machine to Machine Access Tokens issued by Auth0.
        // Once your application gets an Access Token it should keep using it until it expires, to minimize the number of tokens requested.

        /// <summary>Gets the auth0 token.</summary>
        /// <returns></returns>
        private string GetAuth0Token()  // TODO: Move id, secret etc. to appsettings.json and KeyVault.
        {
            try
            {
                var options = new RestClientOptions(_auth0TokenAddress)
                {
                    ThrowOnAnyError = true,
                    Timeout = 1000
                };
                var client = new RestClient(options);
                var request = new RestRequest();
                //request.AddHeader("content-type", "application/json");
                //request.AddParameter("application/json", "{\"client_id\":\"ZKHIqFGxgm9OBc5Bxn1226pT9kXHLknW\",\"client_secret\":\"OpIPkn9Y4Ctoh9UuUbUpGzHybTNVEFevel0yQneY6X5ITKyaRwJEMbHYB3mNofkN\",\"audience\":\"https://bluepenguin.eu.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
                request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=%24%7Baccount.clientId%7D&client_secret=OpIPkn9Y4Ctoh9UuUbUpGzHybTNVEFevel0yQneY6X5ITKyaRwJEMbHYB3mNofkN&audience=ZKHIqFGxgm9OBc5Bxn1226pT9kXHLknW", ParameterType.RequestBody);
                //request.AddParameter("application/json", $"{{\"client_id\":\"{_auth0_Client_id}\",\"client_secret\":\"{_auth0_Client_secret}\",\"audience\":\"{_auth0ApiIdentifier}\",\"grant_type\":\"client_credentials\"}}", ParameterType.RequestBody);
                var response = client.PostAsync(request);

                //var client = new RestClient(_auth0TokenAddress);
                //client.ThrowOnAnyError = true;
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("content-type", "application/json");
                //request.AddParameter("application/json", "{\"client_id\":\"ZKHIqFGxgm9OBc5Bxn1226pT9kXHLknW\",\"client_secret\":\"OpIPkn9Y4Ctoh9UuUbUpGzHybTNVEFevel0yQneY6X5ITKyaRwJEMbHYB3mNofkN\",\"audience\":\"https://bluepenguin.eu.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);


                //var client = new RestClient("https://bluepenguin.eu.auth0.com/oauth/token");
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("content-type", "application/x-www-form-urlencoded");
                //request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=%24%7Baccount.clientId%7D&client_secret=YOUR_CLIENT_SECRET&audience=YOUR_API_IDENTIFIER", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);

                return JsonSerializer.Deserialize<AccessToken>(response.Result.Content).access_token;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
