using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IGroupRepository
    {
        Task CreateGroup(GroupModel group);
        Task<IEnumerable<GroupModel>> GetGroups(CurrentUser currentUser, int skip, int limit);
        Task<GroupModel> GetGroupById(string groupId);
        Task<IEnumerable<GroupModel>> GetGroupsByIds(string[] groupIds);
        Task AddCurrentUserToGroup(CurrentUser currentUser, string groupId);
        Task RemoveCurrentUserFromGroups(string profileId, string[] groupIds);
    }
}
