using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
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
        private readonly string _artemisUrl;
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
            _artemisUrl = config.GetValue<string>("ArtemisUrl");
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

        public async Task<CurrentUser> CreateRandomUser(string accessToken)
        {
            try
            {
                int enumMemberCount = 0;

                var newUser = new CurrentUser();

                newUser.ProfileId = Guid.NewGuid().ToString();
                newUser.CreatedOn = DateTime.UtcNow;
                newUser.UpdatedOn = DateTime.UtcNow;
                newUser.LastActive = DateTime.UtcNow;

                // Initiate empty lists and other defaults
                newUser.Tags ??= new List<string>();
                newUser.Bookmarks = new List<Bookmark>();
                newUser.Images = new List<ImageModel>();
                newUser.Visited = new Dictionary<string, DateTime>();
                newUser.Likes = new List<string>();
                newUser.Complains = new Dictionary<string, DateTime>();
                newUser.Groups = new Dictionary<string, DateTime?>();

                // Add a random name
                if (string.IsNullOrEmpty(newUser.Name))
                {
                    var randomNumber = randomIntFromInterval(0, 1000);
                    var paddedNumber = randomNumber.ToString("D").Length + 4;
                    newUser.Name = "RaU-" + randomNumber.ToString("D" + paddedNumber.ToString());
                }

                //Avatar
                newUser.Avatar = new AvatarModel()
                {
                    CircleColour = "#607D8B",
                    InitialsColour = "#f07537",
                    Initials = newUser.Name.Substring(newUser.Name.Length - 3)
            };

                // Gender
                enumMemberCount = Enum.GetNames(typeof(GenderType)).Length;
                newUser.Gender = (GenderType)randomIntFromInterval(0, enumMemberCount);

                // Seeking
                var seeking = new List<GenderType> { };
                var randomInt = randomIntFromInterval(1, enumMemberCount + 1);
                enumMemberCount = Enum.GetNames(typeof(GenderType)).Length;
                for (int i = 0; i < randomInt; i++)
                {
                    GenderType seekingGender;
                    do
                    {
                        seekingGender = (GenderType)randomIntFromInterval(0, enumMemberCount);
                    } while (seeking.Count > 0 && seeking.Contains(seekingGender));

                    seeking.Add(seekingGender);
                }
                newUser.Seeking = seeking;

                // Countrycode
                newUser.Countrycode = "dk";

                // Age
                newUser.Age = randomIntFromInterval(16, 126);

                // Height
                newUser.Height = randomIntFromInterval(0, 300);

                // Description
                newUser.Description = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

                // Images
                var imageNumber = randomIntFromInterval(1, 441);

                var imageModel = new ImageModel()
                {
                    ImageId = Guid.NewGuid().ToString(),
                    FileName = imageNumber.ToString() + ".webp",
                    Title = newUser.Name,
                };

                newUser.Images = new List<ImageModel> { imageModel };

                await this.CopyImageFromRandomFolderToProfileId(imageNumber, newUser.ProfileId, accessToken);

                // Body
                enumMemberCount = Enum.GetNames(typeof(BodyType)).Length;
                newUser.Body = (BodyType)randomIntFromInterval(0, enumMemberCount + 1);

                // SmokingHabits
                enumMemberCount = Enum.GetNames(typeof(SmokingHabitsType)).Length;
                newUser.SmokingHabits = (SmokingHabitsType)randomIntFromInterval(0, enumMemberCount);

                // HasChildren
                enumMemberCount = Enum.GetNames(typeof(HasChildrenType)).Length;
                newUser.HasChildren = (HasChildrenType)randomIntFromInterval(0, enumMemberCount);

                // WantChildren
                enumMemberCount = Enum.GetNames(typeof(WantChildrenType)).Length;
                newUser.WantChildren = (WantChildrenType)randomIntFromInterval(0, enumMemberCount);

                // HasPets
                enumMemberCount = Enum.GetNames(typeof(HasPetsType)).Length;
                newUser.HasPets = (HasPetsType)randomIntFromInterval(0, enumMemberCount);

                // LivesIn
                enumMemberCount = Enum.GetNames(typeof(LivesInType)).Length;
                newUser.LivesIn = (LivesInType)randomIntFromInterval(0, enumMemberCount);

                // Education
                enumMemberCount = Enum.GetNames(typeof(EducationType)).Length;
                newUser.Education = (EducationType)randomIntFromInterval(0, enumMemberCount);

                // EducationStatus
                enumMemberCount = Enum.GetNames(typeof(EducationStatusType)).Length;
                newUser.EducationStatus = (EducationStatusType)randomIntFromInterval(0, enumMemberCount);

                // EmploymentStatus
                enumMemberCount = Enum.GetNames(typeof(EmploymentStatusType)).Length;
                newUser.EmploymentStatus = (EmploymentStatusType)randomIntFromInterval(0, enumMemberCount);

                // SportsActivity
                enumMemberCount = Enum.GetNames(typeof(SportsActivityType)).Length;
                newUser.SportsActivity = (SportsActivityType)randomIntFromInterval(0, enumMemberCount);

                // EatingHabits
                enumMemberCount = Enum.GetNames(typeof(EatingHabitsType)).Length;
                newUser.EatingHabits = (EatingHabitsType)randomIntFromInterval(0, enumMemberCount);

                // ClotheStyle
                enumMemberCount = Enum.GetNames(typeof(ClotheStyleType)).Length;
                newUser.ClotheStyle = (ClotheStyleType)randomIntFromInterval(0, enumMemberCount);

                // BodyArt
                enumMemberCount = Enum.GetNames(typeof(BodyArtType)).Length;
                newUser.BodyArt = (BodyArtType)randomIntFromInterval(0, enumMemberCount);

                return newUser;
            }
            catch
            {
                throw;
            }
        }

        private int randomIntFromInterval(int min, int max)
        {
            var random = new Random();

            return random.Next(min, max);
        }

        /// <summary>Gets the auth0 token.</summary>
        /// <returns></returns>
        private async Task CopyImageFromRandomFolderToProfileId(int sourceImage, string profileId, string accessToken)
        {
            try
            {
                var client = new RestClient(_artemisUrl);
                var request = new RestRequest(_artemisUrl + $"CopyImageFromRandomFolderToProfileId/{sourceImage}", Method.Post);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + accessToken);
                request.AddParameter("application/json", $"\"{profileId}\"", ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
            }
            catch
            {
                throw;
            }
        }
    }
}
