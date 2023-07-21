using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IFeedbackRepository
    {
        Task AddFeedback(Feedback item);
        Task ToggleFeedbackStatus(string[] feedbackIds, bool status);
        Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(string Countrycode, string Languagecode, int skip, int limit);
        Task AssignFeedbackToAdmin(CurrentUser currentUser, string[] feedbackIds);
        Task<IEnumerable<Feedback>> GetFeedbacksByFilter(FeedbackFilter feedbackFilter, int skip, int limit);
    }
}