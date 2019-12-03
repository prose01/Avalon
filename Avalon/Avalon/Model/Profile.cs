using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Avalon.Model
{
    public class Profile
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string ProfileId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Description { get; set; }
        public List<string> Bookmarks { get; set; }

        //public string[] Tags { get; set; }

        //public string JobTitle { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public GenderType Gender { get; set; }

        [BsonRepresentation(BsonType.String)]
        public BodyType Body { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public SmokingHabitsType SmokingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public AllergiesType Allergies { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public bool HasChildren { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public WantChildrenType WantChildren { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public LocationType LivesIn { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EducationType EducationArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EducationStatusType EducationStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EducationLevelType EducationLevel { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EmploymentStatusType EmploymentStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EmploymentAreaType EmploymentArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EmploymentLevelType EmploymentLevel { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public PoliticalOrientationType PoliticalOrientation { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public SportType Sport { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public EatingHabitsType EatingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public ClotheStyleType ClotheStyle { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public BodyArtType BodyArt { get; set; }
    }


    #region enums

    public enum AllergiesType
    {
        Yes,
        No
    }

    public enum ClotheStyleType
    {
    }

    public enum BodyType
    {
        Atletic,
        Chubby,
        Normal,
        Robust,
        Slim
    }

    public enum BodyArtType
    {
        Piercing,
        Tatoo,
        Other
    }

    public enum GenderType
    {
        Female,
        Male
    }
    
    public enum EatingHabitsType
    {
        Healthy,
        Gastronomic,
        Normal,
        Kosher,
        Organic,
        Traditional,
        Vegetarian
    }

    public enum EducationLevelType
    {
    }

    public enum EducationStatusType
    {
        Graduated,
        Student
    }

    public enum EducationType
    {
    }
    public enum EmploymentLevelType
    {
    }

    public enum EmploymentAreaType
    {
    }

    public enum EmploymentStatusType
    {
    }

    public enum LocationType
    {
    }

    public enum PoliticalOrientationType
    {
    }

    public enum SmokingHabitsType
    {
        [EnumMember(Value = "Non Smoker")]
        NonSmoker,
        Occationally,
        Smoker
    }

    public enum SportType
    {
    }

    public enum WantChildrenType
    {
        Yes,
        No
    }

    #endregion
}
