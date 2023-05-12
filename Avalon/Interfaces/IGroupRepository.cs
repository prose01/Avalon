using Avalon.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IGroupRepository
    {
        Task<GroupModel> GetGroup(string groupId);
        Task<IEnumerable<GroupModel>> GetGroups(string[] groupIds);
        Task RemoveCurrentUserFromGroups(string profileId, string[] groupIds);
    }
}
