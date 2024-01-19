using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace Avalon.Model
{
    public class GroupModel
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public string GroupId { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [StringLength(50, ErrorMessage = "Name length cannot be more than 50 characters long.")]
        public string Name { get; set; }


        [StringLength(500, ErrorMessage = "Description length cannot be more than 500 characters long.")]
        public string Description { get; set; }

        public AvatarModel Avatar { get; set; }

        public string Countrycode { internal get; set; }

        public List<GroupMember> GroupMemberslist { get; set; }
    }
}
