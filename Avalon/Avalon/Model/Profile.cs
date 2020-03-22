using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Avalon.Model
{
    public class Profile : AbstractProfile
    {
        #region special properties
        public List<string> Bookmarks { private get; set; }

        #endregion

        public override ObjectId _id { get; set; }
        public override string Auth0Id { get; set; }
        public override string ProfileId { get; set; }
        public override bool Admin { get; set; } = false;
        public override string Name { get; set; }
        public override DateTime CreatedOn { get; set; }
        public override DateTime UpdatedOn { get; set; }
        public override DateTime LastActive { get; set; }
        public override int? Age { get; set; } = null;
        public override int? Height { get; set; }
        public override int? Weight { get; set; }
        public override string Description { get; set; }

        //public override string[] Tags { get; set; }

        //public override string JobTitle { get; set; } = string.Empty;

        public override GenderType Gender { get; set; }

        public override BodyType Body { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override SmokingHabitsType SmokingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override AllergiesType Allergies { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override bool HasChildren { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override WantChildrenType WantChildren { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override LocationType LivesIn { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EducationType EducationArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EducationStatusType EducationStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EducationLevelType EducationLevel { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EmploymentStatusType EmploymentStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EmploymentAreaType EmploymentArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EmploymentLevelType EmploymentLevel { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override PoliticalOrientationType PoliticalOrientation { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override SportType Sport { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override EatingHabitsType EatingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override ClotheStyleType ClotheStyle { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public override BodyArtType BodyArt { get; set; }
    }
}
