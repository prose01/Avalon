using Avalon.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IFeedbackRepository
    {
        Task AddFeedback(Feedback item);
        Task ToggleFeedbackStatus(string[] feedbackIds, bool status);
        Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(string Countrycode, string Languagecode);
        Task AssignFeedbackToAdmin(CurrentUser currentUser, string[] feedbackIds);
        Task<IEnumerable<Feedback>> GetFeedbacksByFilter(FeedbackFilter feedbackFilter);
        Task<DeleteResult> DeleteOldFeedbacks();
    }
}