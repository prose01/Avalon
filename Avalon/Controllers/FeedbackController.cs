using Avalon.Infrastructure;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ICryptography _cryptography;

        public FeedbackController(IFeedbackRepository reedbackRepository, IHelperMethods helperMethods, ICryptography cryptography)
        {
            _reedbackRepository = reedbackRepository;
            _helper = helperMethods;
            _cryptography = cryptography;
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

                if (currentUser == null)
                {
                    return NotFound();
                }

                item.FeedbackId = Guid.NewGuid().ToString();
                item.DateSent = DateTime.Now;
                item.Open = true;
                item.FromProfileId = currentUser.ProfileId;
                item.FromName = currentUser.Name;
                item.Countrycode = currentUser.Countrycode;
                item.Languagecode = currentUser.Languagecode;

                var encryptedMessage = _cryptography.Encrypt(item.Message);
                item.Message = encryptedMessage;

                await _reedbackRepository.AddFeedback(item);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Open or close the specified feedback.</summary>
        /// <param name="feedbackId">The feedback identifier.</param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        [NoCache]
        [HttpPost("~/OpenFeedbacks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> OpenFeedbacks([FromBody] string[] feedbackIds)
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || !currentUser.Admin)
                {
                    throw new ArgumentException($"Current user is null or does not have admin status.");
                }

                await _reedbackRepository.ToggleFeedbackStatus(feedbackIds, true);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>Open or close the specified feedback.</summary>
        /// <param name="feedbackId">The feedback identifier.</param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        [NoCache]
        [HttpPost("~/CloseFeedbacks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CloseFeedbacks([FromBody] string[] feedbackIds)
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || !currentUser.Admin)
                {
                    throw new ArgumentException($"Current user is null or does not have admin status.");
                }

                await _reedbackRepository.ToggleFeedbackStatus(feedbackIds, false);

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
        /// <param name="countrycode">The Countrycode. Defaults to admin countrycode.</param>
        /// <param name="languagecode">The Languagecode. Defaults to admin languagecode.</param>
        /// <param name="FeedbackType">The FeedbackType.</param>
        /// <param name="feedbackParameters"></param>
        /// <param name="parameterFilter"></param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpGet("~/GetUnassignedFeedbacks/")]
        public async Task<IEnumerable<Feedback>> GetUnassignedFeedbacks([FromQuery] FeedbackParameters feedbackParameters, [FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var feedbacks = await _reedbackRepository.GetUnassignedFeedbacks(feedbackParameters.Countrycode, feedbackParameters.Languagecode, skip, parameterFilter.PageSize);

            foreach (var feedback in feedbacks)
            {
                feedback.Message = _cryptography.Decrypt(feedback.Message);
            }

            return feedbacks;
        }

        /// <summary>
        /// Assign Feedback to admin.
        /// </summary>
        /// <param name="feedbackIds">The feedback identifiers.</param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/AssignFeedbackToAdmin")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> AssignFeedbackToAdmin([FromBody] string[] feedbackIds)
        {
            try
            {
                var currentUser = await _helper.GetCurrentUserProfile(User);

                if (currentUser == null || !currentUser.Admin)
                {
                    throw new ArgumentException($"Current user is null or does not have admin status.");
                }

                await _reedbackRepository.AssignFeedbackToAdmin(currentUser, feedbackIds);

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the specified feedbacks based on a filter. Eg. { FromName: 'someone' }
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="parameterFilter"></param>
        /// <exception cref="ArgumentException">Current user is null or does not have admin status.</exception>
        /// <exception cref="ArgumentException">Current feedbackFilter cannot find any matching feedbacks.. {feedbackFilter}</exception>
        /// <returns></returns>
        [NoCache]
        [HttpPost("~/GetFeedbacksByFilter")]
        public async Task<IEnumerable<Feedback>> GetFeedbacksByFilter([FromBody] RequestBody requestBody, [FromQuery] ParameterFilter parameterFilter)
        {
            var currentUser = await _helper.GetCurrentUserProfile(User);

            if (currentUser == null || !currentUser.Admin)
            {
                throw new ArgumentException($"Current user is null or does not have admin status.");
            }

            var skip = parameterFilter.PageIndex == 0 ? parameterFilter.PageIndex : parameterFilter.PageIndex * parameterFilter.PageSize;

            var feedbacks = await _reedbackRepository.GetFeedbacksByFilter(requestBody.FeedbackFilter, skip, parameterFilter.PageSize) ?? throw new ArgumentException($"Current feedbackFilter cannot find any matching feedbacks.", nameof(requestBody.FeedbackFilter));

            foreach (var feedback in feedbacks)
            {
                feedback.Message = _cryptography.Decrypt(feedback.Message);
            }

            return feedbacks;
        }


        #region Maintenance

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

        #endregion
    }
}
