using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class GroupRepository: IGroupRepository
    {
        private readonly Context _context = null;

        public GroupRepository(IOptions<Settings> settings, IConfiguration config)
        {
            _context = new Context(settings);
        }


        public async Task<GroupModel> GetGroup(string groupId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                    return null;

                var filter = Builders<GroupModel>
                                .Filter.Eq(g => g.GroupId, groupId);

                return await _context.Groups
                    .Find(filter)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Get Groups that CurrentUser is member off.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns>Returns list of groups.</returns>
        public async Task<IEnumerable<GroupModel>> GetGroups(string[] groupIds)
        {
            try
            {
                if (groupIds == null || groupIds.Length == 0)
                    return null;

                var filter = Builders<GroupModel>
                                .Filter.In(g => g.GroupId, groupIds);

                return await _context.Groups
                    .Find(filter)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
