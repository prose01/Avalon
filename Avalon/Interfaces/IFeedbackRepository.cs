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
        Task<IEnumerable<Feedback>> GetUnassignedFeedbacks(FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByProfileId(string profileId, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByStatus(bool status, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByProfileIdAndStatus(bool status, string profileId, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileId(string profileId, FeedbackType type = FeedbackType.Any);
        Task<IEnumerable<Feedback>> GetFeedbacksByAdminProfileIdAndStatus(bool status, string profileId, FeedbackType type = FeedbackType.Any);
        Task<DeleteResult> DeleteOldFeedbacks();
    }
}