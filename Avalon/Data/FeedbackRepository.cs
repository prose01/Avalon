using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly Context _context = null;

        public FeedbackRepository(IOptions<Settings> settings, IConfiguration config)
        {
            _context = new Context(settings);
        }

        public async Task AddFeedback(Feedback item)
        {
            try
            {
                await _context.Feedbacks.InsertOneAsync(item);
            }
            catch
            {
                throw;
            }
        }

        public async Task ToggleFeedbackStatus(string[] feedbackIds, bool status)
        {
            try
            {
                foreach (var feedbackId in feedbackIds)
                {
                    var filter = Builders<Feedback>.Filter.Eq(f => f.FeedbackId, feedbackId);

                    var update = Builders<Feedback>.Update.Set(f => f.Open, status);

                    await _context.Feedbacks.FindOneAndUpdateAsync(filter, update);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets Unassigned Feedbacks.</summary>
        /// <param name="countrycode">The Countrycode. Defaults to admin countrycode.</param>
        /// <param name="languagecode">The Languagecode. Defaults to admin languagecode.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(string countrycode, string languagecode, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>
                {
                    Builders<Feedback>.Filter.Eq(f => f.Open, true),

                    Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, null)
                };

                if (!string.IsNullOrEmpty(countrycode))
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

                if (!string.IsNullOrEmpty(languagecode))
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }


        /// <summary>Assign Feedback to admin.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="feedbackIds">The Feedback identifiers.</param>
        /// <returns></returns>
        public async Task AssignFeedbackToAdmin(CurrentUser currentUser, string[] feedbackIds)
        {
            try
            {
                var filter = Builders<Feedback>.Filter.In(f => f.FeedbackId, feedbackIds);

                List<UpdateDefinition<Feedback>> updates = new List<UpdateDefinition<Feedback>>
                {
                    Builders<Feedback>.Update.Set(f => f.AdminProfileId, currentUser.ProfileId),

                    Builders<Feedback>.Update.Set(f => f.AdminName, currentUser.Name),

                    Builders<Feedback>.Update.Set(f => f.DateSeen, DateTime.UtcNow)
                };

                var combineUpdates = Builders<Feedback>.Update.Combine(updates);

                await _context.Feedbacks.FindOneAndUpdateAsync(filter, combineUpdates);
            }
            catch
            {
                throw;
            }
        }

        // Search for anything in filter - eg. { FromName: 'someone' }
        /// <summary>Gets the feedbacks by filter.</summary>
        /// <param name="feedbackFilter">The feedbackFilter.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Feedback>> GetFeedbacksByFilter(FeedbackFilter feedbackFilter, int skip, int limit)
        {
            try
            {
                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                // Apply all FeedbackFilter criterias.
                var filters = this.ApplyFeedbackFilter(feedbackFilter);

                // If we do not have any filters just return all records
                if (filters.Count == 0 )
                {
                    // Empty filter to get all records
                    var emptyFilter = Builders<Feedback>.Filter.Empty;

                    return await _context.Feedbacks
                                .Find(emptyFilter).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                return await _context.Feedbacks
                                .Find(combineFilters).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Applies definitions to filter.</summary>
        /// <param name="feedbackFilter">The feedbackFilter.</param>
        private List<FilterDefinition<Feedback>> ApplyFeedbackFilter(FeedbackFilter feedbackFilter)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                if (feedbackFilter.FeedbackId != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackId, feedbackFilter.FeedbackId));


                if (feedbackFilter.DateSentStart != null || feedbackFilter.DateSentEnd != null)
                {
                    feedbackFilter.DateSentEnd = feedbackFilter.DateSentEnd.Value.AddDays(1);
                    filters.Add(Builders<Feedback>.Filter.Gt(f => f.DateSent, feedbackFilter.DateSentStart));
                    filters.Add(Builders<Feedback>.Filter.Lte(f => f.DateSent, feedbackFilter.DateSentEnd));
                }

                if (feedbackFilter.DateSeenStart != null || feedbackFilter.DateSeenEnd != null)
                {
                    feedbackFilter.DateSeenEnd = feedbackFilter.DateSeenEnd.Value.AddDays(1);
                    filters.Add(Builders<Feedback>.Filter.Gt(f => f.DateSeen, feedbackFilter.DateSeenStart));
                    filters.Add(Builders<Feedback>.Filter.Lte(f => f.DateSeen, feedbackFilter.DateSeenEnd));
                }

                if (feedbackFilter.FromProfileId != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FromProfileId, feedbackFilter.FromProfileId));

                if (feedbackFilter.FromName != null)
                    filters.Add(Builders<Feedback>.Filter.Regex(f => f.FromName, new BsonRegularExpression(feedbackFilter.FromName, "i")));

                if (!string.IsNullOrEmpty(feedbackFilter.AdminProfileId))
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, feedbackFilter.AdminProfileId));

                if (!string.IsNullOrEmpty(feedbackFilter.AdminName))
                    filters.Add(Builders<Feedback>.Filter.Regex(f => f.AdminName, new BsonRegularExpression(feedbackFilter.AdminName, "i")));

                if (feedbackFilter.FeedbackType != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, feedbackFilter.FeedbackType));

                if (feedbackFilter.Message != null)
                    filters.Add(Builders<Feedback>.Filter.Regex(f => f.Message, new BsonRegularExpression(feedbackFilter.Message, "i")));

                if (feedbackFilter.Open != null)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, feedbackFilter.Open));
                }

                if (feedbackFilter.Countrycode != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, feedbackFilter.Countrycode));

                if (feedbackFilter.Languagecode != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, feedbackFilter.Languagecode));

                return filters;
            }
            catch
            {
                throw;
            }
        }
    }
}
