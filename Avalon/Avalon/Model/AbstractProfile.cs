using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Avalon.Model
{
    [BsonKnownTypes(typeof(CurrentUser), typeof(Profile))]
    public abstract class AbstractProfile
    {
        [BsonId]
        public abstract ObjectId _id { get; set; }
        public abstract string ProfileId { get; set; }
        public abstract string Email { get; set; }
        public abstract string Name { get; set; }
        public abstract DateTime CreatedOn { get; set; }
        public abstract DateTime UpdatedOn { get; set; }
        public abstract DateTime LastActive { get; set; }
        public abstract int Age { get; set; }
        public abstract int Height { get; set; }
        public abstract int Weight { get; set; }
        public abstract string Description { get; set; }

        //public abstract string[] Tags { get; set; }

        //public abstract string JobTitle { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract GenderType Gender { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract BodyType Body { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract SmokingHabitsType SmokingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract AllergiesType Allergies { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract bool HasChildren { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract WantChildrenType WantChildren { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract LocationType LivesIn { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EducationType EducationArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EducationStatusType EducationStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EducationLevelType EducationLevel { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EmploymentStatusType EmploymentStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EmploymentAreaType EmploymentArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EmploymentLevelType EmploymentLevel { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract PoliticalOrientationType PoliticalOrientation { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract SportType Sport { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EatingHabitsType EatingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract ClotheStyleType ClotheStyle { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract BodyArtType BodyArt { get; set; }
    }
}
