using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;
using System;

namespace Avalon.Model
{
    public class GroupModel
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public string GroupId { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string Name { get; set; }

        public string Description { get; set; }

        public AvatarModel Avatar { get; set; }

        public string Countrycode { internal get; set; }

        public List<GroupMember> GroupMemberslist { get; set; }
    }
}
