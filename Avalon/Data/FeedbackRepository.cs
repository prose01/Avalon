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
        private readonly int _deleteFeedbacksOlderThan;

        public FeedbackRepository(IOptions<Settings> settings, IConfiguration config)
        {
            _context = new Context(settings);
            _deleteFeedbacksOlderThan = config.GetValue<int>("DeleteFeedbacksOlderThan");
        }

        public async Task AddFeedback(Feedback item)
        {
            try
            {
                item.DateSent = DateTime.Now;
                item.Open = Boolean.TrueString;

                await _context.Feedbacks.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(string countrycode, string languagecode)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, null));

                if (!string.IsNullOrEmpty(countrycode))
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

                if (!string.IsNullOrEmpty(languagecode))
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
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

                List<UpdateDefinition<Feedback>> updates = new List<UpdateDefinition<Feedback>>();

                updates.Add(Builders<Feedback>.Update.Set(f => f.AdminProfileId, currentUser.ProfileId));

                updates.Add(Builders<Feedback>.Update.Set(f => f.AdminName, currentUser.Name));

                updates.Add(Builders<Feedback>.Update.Set(f => f.DateSeen, DateTime.Now));

                var combineUpdates = Builders<Feedback>.Update.Combine(updates);

                await _context.Feedbacks.FindOneAndUpdateAsync(filter, combineUpdates);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Search for anything in filter - eg. { FromName: 'someone' }
        /// <summary>Gets the feedbacks by filter.</summary>
        /// <param name="feedbackFilter">The feedbackFilter.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Feedback>> GetFeedbacksByFilter(FeedbackFilter feedbackFilter)
        {
            try
            {
                //Apply all FeedbackFilter criterias.
                var filters = this.ApplyFeedbackFilter(feedbackFilter);

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                                .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
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

                if (feedbackFilter.DateSentStart != null)
                    filters.Add(Builders<Feedback>.Filter.Gt(f => f.DateSent, feedbackFilter.DateSentStart));

                if (feedbackFilter.DateSeenEnd != null)
                    filters.Add(Builders<Feedback>.Filter.Lte(f => f.DateSent, feedbackFilter.DateSeenEnd));

                if (feedbackFilter.DateSeenStart != null)
                    filters.Add(Builders<Feedback>.Filter.Gt(f => f.DateSeen, feedbackFilter.DateSeenStart));

                if (feedbackFilter.DateSentEnd != null)
                    filters.Add(Builders<Feedback>.Filter.Lte(f => f.DateSeen, feedbackFilter.DateSentEnd));

                if (feedbackFilter.FromProfileId != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FromProfileId, feedbackFilter.FromProfileId));

                if (feedbackFilter.FromName != null)
                    filters.Add(Builders<Feedback>.Filter.Regex(f => f.FromName, new BsonRegularExpression(feedbackFilter.FromName, "i")));

                if (feedbackFilter.AdminProfileId != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, feedbackFilter.AdminProfileId));

                if (feedbackFilter.AdminName != null)
                    filters.Add(Builders<Feedback>.Filter.Regex(f => f.AdminName, new BsonRegularExpression(feedbackFilter.AdminName, "i")));

                if (feedbackFilter.FeedbackType != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, feedbackFilter.FeedbackType));

                if (feedbackFilter.Message != null)
                    filters.Add(Builders<Feedback>.Filter.Regex(f => f.Message, new BsonRegularExpression(feedbackFilter.Message, "i")));

                if (feedbackFilter.Open != null && feedbackFilter.Open != "notChosen")
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, feedbackFilter.Open));

                if (feedbackFilter.Countrycode != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, feedbackFilter.Countrycode));

                if (feedbackFilter.Languagecode != null)
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, feedbackFilter.Languagecode));

                return filters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

















        //public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileId(string countrycode, string languagecode, string profileId, FeedbackType type = FeedbackType.Any)
        //{
        //    try
        //    {
        //        List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, profileId));

        //        if (type != FeedbackType.Any)
        //        {
        //            filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
        //        }

        //        var combineFilters = Builders<Feedback>.Filter.And(filters);

        //        SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

        //        return await _context.Feedbacks
        //                    .Find(combineFilters).Sort(sortDefinition).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileIdAndStatus(string countrycode, string languagecode, bool status, string profileId, FeedbackType type = FeedbackType.Any)
        //{
        //    try
        //    {
        //        List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, profileId));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, status));

        //        if(type != FeedbackType.Any)
        //        {
        //            filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
        //        }

        //        var combineFilters = Builders<Feedback>.Filter.And(filters);

        //        SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

        //        return await _context.Feedbacks
        //                    .Find(combineFilters).Sort(sortDefinition).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileId(string countrycode, string languagecode, string profileId, FeedbackType type = FeedbackType.Any)
        //{
        //    try
        //    {
        //        List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.FromProfileId, profileId));

        //        if (type != FeedbackType.Any)
        //        {
        //            filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
        //        }

        //        var combineFilters = Builders<Feedback>.Filter.And(filters);

        //        SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

        //        return await _context.Feedbacks
        //                    .Find(combineFilters).Sort(sortDefinition).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileIdAndStatus(string countrycode, string languagecode, bool status, string profileId, FeedbackType type = FeedbackType.Any)
        //{
        //    try
        //    {
        //        List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.FromProfileId, profileId));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, status));

        //        if (type != FeedbackType.Any)
        //        {
        //            filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
        //        }

        //        var combineFilters = Builders<Feedback>.Filter.And(filters);

        //        SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

        //        return await _context.Feedbacks
        //                    .Find(combineFilters).Sort(sortDefinition).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //public async Task<IEnumerable<Feedback>> GetFeedbacksByStatus(string countrycode, string languagecode, bool status, FeedbackType type = FeedbackType.Any)
        //{
        //    try
        //    {
        //        List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Countrycode, countrycode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Languagecode, languagecode));

        //        filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, status));

        //        if (type != FeedbackType.Any)
        //        {
        //            filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
        //        }

        //        var combineFilters = Builders<Feedback>.Filter.And(filters);

        //        SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

        //        return await _context.Feedbacks
        //                    .Find(combineFilters).Sort(sortDefinition).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        /// <summary>Deletes Feedbacks that are greater  than 1 year old (DateSeen) and closed.</summary>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteOldFeedbacks()
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Gt(f => f.DateSeen, DateTime.Now.AddYears(-_deleteFeedbacksOlderThan)));

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, Boolean.FalseString));

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                return await _context.Feedbacks.DeleteManyAsync(combineFilters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
