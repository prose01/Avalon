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
        /// <param name="FeedbackType"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetUnassignedFeedbacks/")]
        public async Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(FeedbackType type = FeedbackType.Any)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetUnassignedFeedbacks(type);
        }

        /// <summary>
        /// Gets Feedbacks By Admin ProfileId.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Admin ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByAdminProfileId/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileId(string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"Admin ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByAdminProfileId(profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By Admin ProfileId and Status.
        /// </summary>
        /// <param name="status">The Feedback status (true = open, false = closed, default = false).</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Admin ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByAdminProfileIdAndStatus/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileIdAndStatus(bool status, string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"Admin ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByAdminProfileIdAndStatus(status, profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By ProfileId.
        /// </summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByProfileId/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileId(string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByProfileId(profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By ProfileId and Status.
        /// </summary>
        /// <param name="status">The Feedback status (true = open, false = closed, default = false).</param>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Admin ProfileId is null.</exception>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByProfileIdAndStatus/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileIdAndStatus(bool status, string profileId, FeedbackType type = FeedbackType.Any)
        {
            if (string.IsNullOrEmpty(profileId)) throw new ArgumentException($"ProfileId is null.", nameof(profileId));

            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByProfileIdAndStatus(status, profileId, type);
        }

        /// <summary>
        /// Gets Feedbacks By Status.
        /// </summary>
        /// <param name="status">The Feedback status (true = open, false = closed, default = false).</param>
        /// <param name="type"> The FeedbackType.</param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetFeedbacksByStatus/")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByStatus(bool status, FeedbackType type = FeedbackType.Any)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            return await _reedbackRepository.GetFeedbacksByStatus(status, type);
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
