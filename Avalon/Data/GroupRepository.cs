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

        /// <summary>Remove CurrentUser from groups.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="groupIds">The group ids.</param>
        public async Task RemoveCurrentUserFromGroups(string profileId, string[] groupIds)
        {
            try
            {
                var update = Builders<GroupModel>.Update;
                var updates = new List<UpdateDefinition<GroupModel>>();

                var groups = await this.GetGroups(groupIds);

                foreach (var group in groups)
                {
                    foreach (var member in group.GroupMemberslist)
                    {
                        if (member.ProfileId == profileId)
                        {
                            group.GroupMemberslist.Remove(member);

                            updates.Add(update.Set(g => g.GroupMemberslist, group.GroupMemberslist));

                            break;
                        }
                    }
                }

                var filter = Builders<GroupModel>
                                .Filter.In(g => g.GroupId, groupIds);

                if (updates.Count == 0)
                    return;

                await _context.Groups.UpdateManyAsync(filter, update.Combine(updates));

            }
            catch
            {
                throw;
            }
        }
    }
}
