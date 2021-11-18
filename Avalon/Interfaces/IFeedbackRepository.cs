using Avalon.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IFeedbackRepository
    {
        Task AddFeedback(Feedback item);
        //Task<IEnumerable<Feedback>> GetFeedbacks();
        Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(string Countrycode, string Languagecode, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByProfileId(string Countrycode, string Languagecode, string profileId, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByStatus(string Countrycode, string Languagecode, bool status, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByProfileIdAndStatus(string Countrycode, string Languagecode, bool status, string profileId, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileId(string Countrycode, string Languagecode, string profileId, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileIdAndStatus(string Countrycode, string Languagecode, bool status, string profileId, FeedbackType type = FeedbackType.Any);
        Task<DeleteResult> DeleteOldFeedbacks();
    }
}