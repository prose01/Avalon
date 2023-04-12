using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Avalon.Model
{
    public class Profile : AbstractProfile
    {
        #region special properties

        [BsonRepresentation(BsonType.String)]
        internal GenderType Gender { get; set; }

        [BsonRepresentation(BsonType.String)]
        internal List<GenderType> Seeking { get; set; }

        public string Languagecode { private get; set; }
        public List<string> Bookmarks { private get; set; }
        public List<ChatMember> ChatMemberslist { internal get; set; }
        public ProfileFilter ProfileFilter { private get; set; }
        public Dictionary<string, DateTime> Visited { internal get; set; }
        public Dictionary<string, DateTime> IsBookmarked { internal get; set; }
        public List<string> Likes { get; set; }
        public Dictionary<string, DateTime> Complains { internal get; set; }
        public List<string> Groups { get; set; }
        #endregion

        internal override ObjectId _id { get; set; }
        public override string Auth0Id { internal get; set; }
        public override string ProfileId { get; set; }
        public override bool Admin { get; set; } = false;
        public override string Name { get; set; }
        public override AvatarModel Avatar { get; set; }
        public override DateTime CreatedOn { get; set; }
        public override DateTime UpdatedOn { get; set; }
        public override DateTime LastActive { get; set; }
        public override string Countrycode { get; set; }
        public override int? Age { get; set; } = null;
        public override int? Height { get; set; }
        public override bool Contactable { get; set; } = true;
        public override string Description { get; set; }
        public override List<ImageModel> Images { get; set; }
        public override List<string> Tags { get; set; }
        public override BodyType Body { get; set; }
        public override SmokingHabitsType SmokingHabits { get; set; }
        public override HasChildrenType HasChildren { get; set; }
        public override WantChildrenType WantChildren { get; set; }
        public override HasPetsType HasPets { get; set; }
        public override LivesInType LivesIn { get; set; }
        public override EducationType Education { get; set; }
        public override EducationStatusType EducationStatus { get; set; }
        public override EmploymentStatusType EmploymentStatus { get; set; }
        public override SportsActivityType SportsActivity { get; set; }
        public override EatingHabitsType EatingHabits { get; set; }
        public override ClotheStyleType ClotheStyle { get; set; }
        public override BodyArtType BodyArt { get; set; }
    }
}