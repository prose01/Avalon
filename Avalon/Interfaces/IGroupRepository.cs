using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IGroupRepository
    {
        Task<IEnumerable<GroupModel>> GetGroups(CurrentUser currentUser, int skip, int limit);
        Task<GroupModel> GetGroupById(string groupId);
        Task<IEnumerable<GroupModel>> GetGroupsByIds(string[] groupIds);
        Task RemoveCurrentUserFromGroups(string profileId, string[] groupIds);
    }
}
