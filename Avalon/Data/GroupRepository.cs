using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalon.Data
{
    public class GroupRepository: IGroupRepository
    {
        private readonly Context _context = null;
        private readonly double _maxGroupComplainPercentage;
        private int _groupComplainsDaysBack;

        public GroupRepository(IOptions<Settings> settings, IConfiguration config)
        {
            _context = new Context(settings);
            _maxGroupComplainPercentage = config.GetValue<double>("MaxGroupComplainsPercentage");
            _groupComplainsDaysBack = config.GetValue<int>("GroupComplainsDaysBack");
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
        public async Task<(int total, IReadOnlyList<GroupModel> groups)> GetGroups(CurrentUser currentUser, int skip, int limit)
        {
            try
            {
                List<FilterDefinition<GroupModel>> filters = new List<FilterDefinition<GroupModel>>
                {
                    Builders<GroupModel>.Filter.Eq(g => g.Countrycode, currentUser.Countrycode),

                    Builders<GroupModel>.Filter.Where(g => !g.GroupMemberslist.Any(m => m.ProfileId == currentUser.ProfileId && m.Blocked))
                };

                var combineFilters = Builders<GroupModel>.Filter.And(filters);

                //Get total number of groups and groups mathching the filters.
                return await this.GetTotalAndGroups(combineFilters, OrderByType.Name, skip, limit);
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

                SortDefinition<GroupModel> sortDefinition = Builders<GroupModel>.Sort.Ascending(g => g.Name);

                return await _context.Groups
                    .Find(filter).Sort(sortDefinition).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Get Groups by group ids.</summary>
        /// <param name="groupIds">The group ids.</param>
        /// <returns>Returns list of groups.</returns>
        public async Task<(int total, IReadOnlyList<GroupModel> groups)> GetGroupsByFilter(CurrentUser currentUser, string filter, int skip, int limit)
        {
            try
            {
                var filters = Builders<GroupModel>.Filter.And(
                    Builders<GroupModel>.Filter.Eq(g => g.Countrycode, currentUser.Countrycode),
                    Builders<GroupModel>.Filter.Where(g => !g.GroupMemberslist.Any(m => m.ProfileId == currentUser.ProfileId && m.Blocked))
                    ) &
                    Builders<GroupModel>.Filter.Or(
                    Builders<GroupModel>.Filter.Regex(g => g.Name, new BsonRegularExpression(filter, "i")),
                    Builders<GroupModel>.Filter.Regex(g => g.Description, new BsonRegularExpression(filter, "i"))
                    );

                var combineFilters = Builders<GroupModel>.Filter.And(filters);

                //Get total number of groups and groups mathching the filters.
                return await this.GetTotalAndGroups(combineFilters, OrderByType.Name, skip, limit);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Gets the total page count and profiles matching filter.</summary>
        /// <param name="combineFilters">The combine Filters.</param>
        /// <param name="orderByType">The OrderByDescending column type.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        private async Task<(int total, IReadOnlyList<GroupModel> groups)> GetTotalAndGroups(FilterDefinition<GroupModel> combineFilters, OrderByType orderByType, int skip, int limit)
        {
            try
            {
                SortDefinition<GroupModel> sortDefinition;

                switch (orderByType)
                {
                    case OrderByType.Name:
                        sortDefinition = Builders<GroupModel>.Sort.Ascending(g => g.Name);
                        break;
                    default:
                        sortDefinition = Builders<GroupModel>.Sort.Ascending(g => g.Name);
                        break;
                }

                var countFacet = AggregateFacet.Create("count",
                    PipelineDefinition<GroupModel, AggregateCountResult>.Create(new[]
                    {
                        PipelineStageDefinitionBuilder.Count<GroupModel>()
                    }));

                var dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<GroupModel, GroupModel>.Create(new[]
                    {
                        PipelineStageDefinitionBuilder.Sort(sortDefinition),
                        PipelineStageDefinitionBuilder.Skip<GroupModel>(skip),
                        PipelineStageDefinitionBuilder.Limit<GroupModel>(limit),
                    }));

                var aggregation = await _context.Groups.Aggregate()
                    .Match(combineFilters)
                    .Project<GroupModel>(this.GetProjection())
                    .Facet(countFacet, dataFacet)
                    .ToListAsync();

                var count = aggregation.First()
                    .Facets.First(x => x.Name == "count")
                    .Output<AggregateCountResult>()
                    ?.FirstOrDefault()
                    ?.Count ?? 0;

                //var totalPages = ((int)count + limit - 1) / limit;
                var total = (int)count;

                var data = aggregation.First()
                    .Facets.First(x => x.Name == "data")
                    .Output<GroupModel>();

                return (total, data);
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

        /// <summary>Remove User from groups.</summary>
        /// <param name="profileId">The profile identifier.</param>
        /// <param name="groupIds">The group ids.</param>
        public async Task RemoveUserFromGroups(string profileId, string[] groupIds)
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
                            // Do Not remove member if it has been block as this would otherwise allow them to join again.
                            if(member.Blocked == true)
                                break;

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

        /// <summary>Add complain to groupMember for group.</summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="groupId">The group id.</param>
        /// <param name="profileId">The profile identifier.</param>
        public async Task AddComplainToGroupMember(CurrentUser currentUser, string groupId, string profileId)
        {
            try
            {
                var update = Builders<GroupModel>.Update;
                var updates = new List<UpdateDefinition<GroupModel>>();

                var group = await this.GetGroupById(groupId);

                foreach (var member in group.GroupMemberslist)
                {
                    if (member.ProfileId == profileId)
                    {

                        // Remove old complains
                        var oldcomplains = member.Complains.Where(i => i.Value < DateTime.UtcNow.AddDays(-_groupComplainsDaysBack)).ToArray();

                        if (oldcomplains.Length > 0)
                        {
                            foreach (var oldcomplain in oldcomplains)
                            {
                                member.Complains.Remove(oldcomplain.Key.ToString());
                            }
                        }

                        // Add new or update complain
                        if (member.Complains.ContainsKey(currentUser.ProfileId))
                        {
                            member.Complains[currentUser.ProfileId] = DateTime.UtcNow;
                        }
                        else
                        {
                            var complainPair = from pair in member.Complains
                                               orderby pair.Value descending
                                               select pair;


                            member.Complains.Add(currentUser.ProfileId, DateTime.UtcNow);

                            // Block member if too many complains
                            if ((double)member.Complains.Count / group.GroupMemberslist.Count >= _maxGroupComplainPercentage)
                            {
                                member.Blocked = true;
                            }
                        }

                        updates.Add(update.Set(g => g.GroupMemberslist, group.GroupMemberslist));

                        break;
                    }
                }

                var filter = Builders<GroupModel>
                           .Filter.Eq(g => g.GroupId, groupId);

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
                "GroupMemberslist:0, " +      // TODO: See if we can get this back so not to show too much info to users and for perfomance. However, GroupsListviewComponent show GroupBlocked should still work.
                "}";

            return projection;
        }
    }
}
