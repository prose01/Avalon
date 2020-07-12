﻿using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class CurrentUserController : Controller
    {
        private readonly ICurrentUserRepository _profileRepository;
        private readonly IProfilesQueryRepository _profilesQueryRepository;
        private readonly IHelperMethods _helper;
        private readonly long _maxImageNumber;

        public CurrentUserController(IConfiguration config, ICurrentUserRepository profileRepository, IProfilesQueryRepository profilesQueryRepository, IHelperMethods helperMethods)
        {
            _maxImageNumber = config.GetValue<long>("MaxImageNumber");
            _profileRepository = profileRepository;
            _profilesQueryRepository = profilesQueryRepository;
            _helper = helperMethods;
        }

        /// <summary>
        /// Gets the current user profile.
        /// </summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/CurrentUser")]
        public async Task<CurrentUser> GetCurrentUserProfile()
        {
            return await _helper.GetCurrentUserProfile(User);
        }

        /// <summary>
        /// Add new profile to database
        /// </summary>
        /// <param name="profile"> The value.</param>
        [HttpPost("~/CurrentUser")]
        public async Task<IActionResult> Post([FromBody] CurrentUser item)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // Check if Auth0Id exists
                var auth0Id = _helper.GetCurrentUserAuth0Id(User);

                if (string.IsNullOrEmpty(auth0Id)) return BadRequest();

                // Check if Name already exists.
                if (_profilesQueryRepository.GetProfileByName(item.Name).Result != null) return BadRequest();

                // Check if Auth0Id already exists.
                if (_profilesQueryRepository.GetProfileByAuth0Id(auth0Id).Result != null) return BadRequest();

                // Set admin default to false! Only other admins can give this privilege.
                item.Admin = false;

                item.Auth0Id = auth0Id;

                // Initiate empty lists and other default 
                item.Bookmarks = new List<string>(); 
                item.ChatMemberslist = new List<ChatMember>();
                item.ProfileFilter = this.CreateBasicProfileFilter(item);
                item.Images = new List<ImageModel>();

                return Ok(_profileRepository.AddProfile(item));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Patches the specified profile identifier. Does not work!!!
        /// </summary>
        /// <param name="patch">The patch.</param>
        //[HttpPatch("~/CurrentUser/")]
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
        [HttpPut("~/CurrentUser")]
        public async Task<IActionResult> Put([FromBody] CurrentUser item)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser.ProfileId != item.ProfileId) return BadRequest();

                // Certain properties cannot be changed by the user.
                item._id = currentUser._id; // _id is immutable and the type is unknow by BluePenguin.
                item.Admin = currentUser.Admin; // No user is allowed to set themselves as Admin!
                item.Name = currentUser.Name; // You cannot change your name after create.
                item.Bookmarks = currentUser.Bookmarks;
                item.ChatMemberslist = currentUser.ChatMemberslist;
                item.ProfileFilter = currentUser.ProfileFilter;
                item.Images = currentUser.Images;
                item.CreatedOn = currentUser.CreatedOn;

                // TODO: Update ProfileFilter with sexualOrientation and Gender when CurrentUser is updated!

                return Ok(_profileRepository.UpdateProfile(item));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the CurrentUser profile.
        /// </summary>
        [HttpDelete("~/CurrentUser")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser.Admin) return BadRequest(); // Amins cannot delete themseleves.

            try
            {
                //await _helper.DeleteProfile(currentUser.ProfileId);
                await _profileRepository.DeleteCurrentUser(currentUser.ProfileId);
                //_imageUtil.DeleteAllImagesForCurrentUser(currentUser);  // Call Artemis

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Saves the profile filter to currentUser.</summary>
        /// <param name="profileFilter">The profile filter.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/SaveProfileFilter")]
        public async Task<IActionResult> SaveProfileFilter([FromBody]ProfileFilter profileFilter)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileFilter == null) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                return Ok(_profileRepository.SaveProfileFilter(currentUser, profileFilter));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Load the profile filter from currentUser.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/LoadProfileFilter")]
        public async Task<ProfileFilter> LoadProfileFilter()
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            return await _profileRepository.LoadProfileFilter(currentUser);
        }

        /// <summary>Adds the profiles to currentUser bookmarks and ChatMemberslist.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/AddProfilesToBookmarks")]
        public async Task<IActionResult> AddProfilesToBookmarks([FromBody] string[] profileIds)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                await _profileRepository.AddProfilesToBookmarks(currentUser, profileIds);
                await _profileRepository.AddProfilesToChatMemberslist(currentUser, profileIds);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }


        /// <summary>Removes the profiles from currentUser bookmarks and ChatMemberslist.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/RemoveProfilesFromBookmarks")]
        public async Task<IActionResult> RemoveProfilesFromBookmarks([FromBody] string[] profileIds)
        {
            if (!ModelState.IsValid) throw new ArgumentException($"ModelState is not valid {ModelState.IsValid}.", nameof(profileIds));
            if (profileIds == null || profileIds.Length < 1) throw new ArgumentException($"ProfileIds is either null {profileIds} or length is < 1 {profileIds.Length}.", nameof(profileIds));

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                await _profileRepository.RemoveProfilesFromBookmarks(currentUser, profileIds);
                await _profileRepository.RemoveProfilesFromChatMemberslist(currentUser, profileIds);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Blocks or unblocks chatMembers.</summary>
        /// <param name="profileIds">The profile ids.</param>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/BlockChatMembers")]
        public async Task<IActionResult> BlockChatMembers([FromBody] string[] profileIds)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (profileIds == null || profileIds.Length < 1) return BadRequest();

            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                await _profileRepository.BlockChatMembers(currentUser, profileIds);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        ///// <summary>Upload images to the profile image folder.</summary>
        ///// <param name="image"></param>
        ///// <param name="title"></param>
        ///// <exception cref="ArgumentException">ModelState is not valid {ModelState.IsValid}. - image</exception>
        ///// <exception cref="ArgumentException">Image length is < 1 {image.Length}. - image</exception>
        //[NoCache]
        //[HttpPost("~/UploadImage")]
        //public async Task<IActionResult> UploadImage([FromForm] IFormFile image, [FromForm] string title)
        //{
        //    if (!ModelState.IsValid) throw new ArgumentException($"ModelState is not valid {ModelState.IsValid}.", nameof(image));
        //    if (image.Length < 0) throw new ArgumentException($"Image length is < 1 {image.Length}.", nameof(image));

        //    try
        //    {
        //        var currentUser = await _helper.GetCurrentUserProfile(User);

        //        if (currentUser.Images.Count >= _maxImageNumber) return BadRequest();

        //        return Ok(_imageUtil.AddImageToCurrentUser(currentUser, image, title));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.ToString());
        //    }
        //}



        ///// <summary>Deletes the image from current user.</summary>
        ///// <param name="imageId">The image identifier.</param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentException">ModelState is not valid {ModelState.IsValid}. - imageId</exception>
        //[NoCache]
        //[HttpPost("~/DeleteImage")]
        //public async Task<IActionResult> DeleteImage([FromBody] string[] imageIds)
        //{
        //    if (!ModelState.IsValid) throw new ArgumentException($"ModelState is not valid {ModelState.IsValid}.", nameof(imageIds));

        //    try
        //    {
        //        var currentUser = await _helper.GetCurrentUserProfile(User);

        //        foreach (var imageId in imageIds)
        //        {
        //            if (!currentUser.Images.Any(i => i.ImageId != imageId)) return BadRequest();
        //        }

        //        return Ok(_imageUtil.DeleteImagesFromCurrentUser(currentUser, imageIds));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.ToString());
        //    }
        //}

        ///// <summary>Gets an images from CurrentUser by Image fileName.</summary>
        ///// <param name="fileName">The image fileName.</param>
        ///// <returns></returns>
        //[NoCache]
        //[HttpGet("~/GetImageByFileName/{fileName}")]
        //public async Task<IActionResult> GetImageByFileName(string fileName)
        //{
        //    try
        //    {
        //        var currentUser = await _helper.GetCurrentUserProfile(User);

        //        if (!currentUser.Images.Any(i => i.FileName == fileName)) return BadRequest();

        //        return Ok(await _imageUtil.GetImageByFileName(currentUser.ProfileId, fileName));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #region Helper methods

        /// <summary>Creates the basic profile filter.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        private ProfileFilter CreateBasicProfileFilter(CurrentUser currentUser)
        {
            var filter = new ProfileFilter();

            switch (currentUser.SexualOrientation)
            {
                case SexualOrientationType.Heterosexual:
                    filter.SexualOrientation = SexualOrientationType.Heterosexual;
                    filter.Gender = currentUser.Gender == GenderType.Male ? GenderType.Female : GenderType.Male;
                    break;
                case SexualOrientationType.Homosexual:
                    filter.SexualOrientation = SexualOrientationType.Homosexual;
                    filter.Gender = currentUser.Gender;
                    break;
                case SexualOrientationType.Bisexual:
                    filter.SexualOrientation = SexualOrientationType.Bisexual;
                    break;
                case SexualOrientationType.Asexual:
                    filter.SexualOrientation = SexualOrientationType.Asexual;
                    break;
                default:
                    filter.SexualOrientation = SexualOrientationType.Heterosexual;
                    filter.Gender = currentUser.Gender == GenderType.Male ? GenderType.Female : GenderType.Male;
                    break;
            }

            return filter;
        }

        #endregion
    }
}
