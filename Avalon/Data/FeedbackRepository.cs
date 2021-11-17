using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
                item.Open = true;

                await _context.Feedbacks.InsertOneAsync(item);
            }
            catch
            {
                throw;
            }
        }

        //public async Task<IEnumerable<Feedback>> GetFeedbacks()
        //{
        //    try
        //    {
        //        SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

        //        return await _context.Feedbacks
        //                    .Find(_ => true).Sort(sortDefinition).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(FeedbackType type = FeedbackType.Any)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, null));

                if (type != FeedbackType.Any)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileId(string profileId, FeedbackType type = FeedbackType.Any)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, profileId));

                if (type != FeedbackType.Any)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileIdAndStatus(bool status, string profileId, FeedbackType type = FeedbackType.Any)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.AdminProfileId, profileId));

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, status));

                if(type != FeedbackType.Any)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileId(string profileId, FeedbackType type = FeedbackType.Any)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.FromProfileId, profileId));

                if (type != FeedbackType.Any)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByProfileIdAndStatus(bool status, string profileId, FeedbackType type = FeedbackType.Any)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.FromProfileId, profileId));

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, status));

                if (type != FeedbackType.Any)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByStatus(bool status, FeedbackType type = FeedbackType.Any)
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, status));

                if (type != FeedbackType.Any)
                {
                    filters.Add(Builders<Feedback>.Filter.Eq(f => f.FeedbackType, type));
                }

                var combineFilters = Builders<Feedback>.Filter.And(filters);

                SortDefinition<Feedback> sortDefinition = Builders<Feedback>.Sort.Descending(f => f.DateSent);

                return await _context.Feedbacks
                            .Find(combineFilters).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Deletes Feedbacks that are greater  than 1 year old (DateSeen) and closed.</summary>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteOldFeedbacks()
        {
            try
            {
                List<FilterDefinition<Feedback>> filters = new List<FilterDefinition<Feedback>>();

                filters.Add(Builders<Feedback>.Filter.Gt(f => f.DateSeen, DateTime.Now.AddYears(-_deleteFeedbacksOlderThan)));

                filters.Add(Builders<Feedback>.Filter.Eq(f => f.Open, false));

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
