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
    public class GroupRepository: IGroupRepository
    {
        private readonly Context _context = null;

        public GroupRepository(IOptions<Settings> settings, IConfiguration config)
        {
            _context = new Context(settings);
        }



        /// <summary>Create chat group.</summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public async Task CreateGroup(GroupModel group)
        {
            try
            {
                group.CreatedOn = DateTime.UtcNow;

                await _context.Groups.InsertOneAsync(group);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Get all groups with same countrycode as currentUser.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public async Task<IEnumerable<GroupModel>> GetGroups(CurrentUser currentUser, int skip, int limit)
        {
            try
            {
                var filter = Builders<GroupModel>
                                .Filter.Eq(g => g.Countrycode, currentUser.Countrycode);

                SortDefinition<GroupModel> sortDefinition = Builders<GroupModel>.Sort.Ascending(g => g.Name);

                return await _context.Groups
                            .Find(filter).Project<GroupModel>(this.GetProjection()).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            }
            catch
            {
                throw;
            }
        }



        /// <summary>Get Group by groupId.</summary>
        /// <param name="groupIds">groupId.</param>
        /// <returns>Returns group.</returns>
        public async Task<GroupModel> GetGroupById(string groupId)
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

        /// <summary>Get Groups by group ids.</summary>
        /// <param name="groupIds">The group ids.</param>
        /// <returns>Returns list of groups.</returns>
        public async Task<IEnumerable<GroupModel>> GetGroupsByIds(string[] groupIds)
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

        /// <summary>Add CurrentUser to group.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="groupId">The group id.</param>
        public async Task AddCurrentUserToGroup(CurrentUser currentUser, string groupId)
        {
            try
            {
                var member = new GroupMember() {
                    ProfileId = currentUser.ProfileId,
                    Name = currentUser.Name,
                    Blocked = false 
                }; 

                var filter = Builders<GroupModel>
                                .Filter.Eq(g => g.GroupId, groupId);

                var update = Builders<GroupModel>
                                .Update.Push(g => g.GroupMemberslist, member);

                await _context.Groups.FindOneAndUpdateAsync(filter, update);
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

                var groups = await this.GetGroupsByIds(groupIds);

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

        private ProjectionDefinition<GroupModel> GetProjection()
        {
            ProjectionDefinition<GroupModel> projection = "{ " +
                "_id: 0, " +
                "Countrycode: 0, " +
                "GroupMemberslist:0, " +
                "}";

            return projection;
        }
    }
}
