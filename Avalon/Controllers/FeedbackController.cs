using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Avalon.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackRepository _reedbackRepository;
        private readonly IHelperMethods _helper;

        public FeedbackController(IFeedbackRepository reedbackRepository, IHelperMethods helperMethods)
        {
            _reedbackRepository = reedbackRepository;
            _helper = helperMethods;
        }

        /// <summary>
        /// Add new feedback to database
        /// </summary>
        /// <param name="Feedback"> The Feedback.</param>
        [HttpPost("~/AddFeedback")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromBody] Feedback item)
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || !currentUser.Admin)
                {
                    return NotFound();
                }

                await _reedbackRepository.AddFeedback(item);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Gets Unassigned Feedbacks.
        /// </summary>
        /// <param name="countrycode">The Countrycode</param>
        /// <param name="languagecode">The Languagecode</param>
        /// <param name="FeedbackType">The FeedbackType.</param>
        /// <exception cref="ArgumentException">Countrycode is null.</exception>
        /// <exception cref="ArgumentException">Languagecode is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetUnassignedFeedbacks/")]
        public async Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(string countrycode, string languagecode, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(countrycode)) throw new ArgumentException($"Countrycode is null.", nameof(countrycode));
            if (string.IsNullOrEmpty(languagecode)) throw new ArgumentException($"Languagecode is null.", nameof(languagecode));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetUnassignedFeedbacks(countrycode, languagecode, type);
        }

        /// <summary>
        /// Gets Feedbacks By Admin ProfileId.
        /// </summary>
        /// <param name="countrycode">The Countrycode</param>
        /// <param name="languagecode">The Languagecode</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Countrycode is null.</exception>
        /// <exception cref="ArgumentException">Languagecode is null.</exception>
        /// <exception cref="ArgumentException">Admin ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByAdminProfileId/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileId(string countrycode, string languagecode, string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(countrycode)) throw new ArgumentException($"Countrycode is null.", nameof(countrycode));
            if (string.IsNullOrEmpty(languagecode)) throw new ArgumentException($"Languagecode is null.", nameof(languagecode));
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"Admin ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByAdminProfileId(countrycode, languagecode, profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By Admin ProfileId and Status.
        /// </summary>
        /// <param name="countrycode">The Countrycode</param>
        /// <param name="languagecode">The Languagecode</param>
        /// <param name="status">The Feedback status (true = open, false = closed, default = false).</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Countrycode is null.</exception>
        /// <exception cref="ArgumentException">Languagecode is null.</exception>
        /// <exception cref="ArgumentException">Admin ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByAdminProfileIdAndStatus/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileIdAndStatus(string countrycode, string languagecode, bool status, string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(countrycode)) throw new ArgumentException($"Countrycode is null.", nameof(countrycode));
            if (string.IsNullOrEmpty(languagecode)) throw new ArgumentException($"Languagecode is null.", nameof(languagecode));
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"Admin ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByAdminProfileIdAndStatus(countrycode, languagecode, status, profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By ProfileId.
        /// </summary>
        /// <param name="countrycode">The Countrycode</param>
        /// <param name="languagecode">The Languagecode</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Countrycode is null.</exception>
        /// <exception cref="ArgumentException">Languagecode is null.</exception>
        /// <exception cref="ArgumentException">ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByProfileId/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileId(string countrycode, string languagecode, string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(countrycode)) throw new ArgumentException($"Countrycode is null.", nameof(countrycode));
            if (string.IsNullOrEmpty(languagecode)) throw new ArgumentException($"Languagecode is null.", nameof(languagecode));
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByProfileId(countrycode, languagecode, profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By ProfileId and Status.
        /// </summary>
        /// <param name="countrycode">The Countrycode</param>
        /// <param name="languagecode">The Languagecode</param>
        /// <param name="status">The Feedback status (true = open, false = closed, default = false).</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Countrycode is null.</exception>
        /// <exception cref="ArgumentException">Languagecode is null.</exception>
        /// <exception cref="ArgumentException">Admin ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByProfileIdAndStatus/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileIdAndStatus(string countrycode, string languagecode, bool status, string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(countrycode)) throw new ArgumentException($"Countrycode is null.", nameof(countrycode));
            if (string.IsNullOrEmpty(languagecode)) throw new ArgumentException($"Languagecode is null.", nameof(languagecode));
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByProfileIdAndStatus(countrycode, languagecode, status, profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By Status.
        /// </summary>
        /// <param name="countrycode">The Countrycode</param>
        /// <param name="languagecode">The Languagecode</param>
        /// <param name="status">The Feedback status (true = open, false = closed, default = false).</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Countrycode is null.</exception>
        /// <exception cref="ArgumentException">Languagecode is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByStatus/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByStatus(string countrycode, string languagecode, bool status, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(countrycode)) throw new ArgumentException($"Countrycode is null.", nameof(countrycode));
            if (string.IsNullOrEmpty(languagecode)) throw new ArgumentException($"Languagecode is null.", nameof(languagecode));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByStatus(countrycode, languagecode, status, type);
        }

        /// <summary>Deletes Feedbacks that are greater  than 1 year old (DateSeen) and closed.</summary>
        /// <returns></returns>
        [NoCache]
        [HttpDelete("~/DeleteOldFeedbacks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteOldFeedbacks()
        {
            try
            {
                //await _reedbackRepository.DeleteOldFeedbacks();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
    }
}
