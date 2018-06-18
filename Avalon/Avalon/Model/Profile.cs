using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public string Name { get; set; } = string.Empty;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public List<string> Bookmarks { get; set; }
        public GenderType Gender { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        //Skal ændres til BodyType
        public string Body { get; set; } = string.Empty;
        public string Descruption { get; set; } = string.Empty;
        //public SmokingHabitsType SmokingHabits { get; set; }
        //public AllergiesType Allergies { get; set; }
        //public bool HasChildren { get; set; }
        //public WantChildrenType WantChildren { get; set; }
        //public LocationType LivesIn { get; set; }
        //public EducationType EducationArea { get; set; }
        //public EducationStatusType EducationStatus { get; set; }
        //public EducationLevelType EducationLevel { get; set; }
        //public EmploymentStatusType EmploymentStatus { get; set; }
        //public EmploymentAreaType EmploymentArea { get; set; }
        //public EmploymentLevelType EmploymentLevel { get; set; }
        //public string JobTitle { get; set; } = string.Empty;
        //public PoliticalOrientationType PoliticalOrientation { get; set; }
        //public SportType Sport { get; set; }
        //public EatingHabitsType EatingHabits { get; set; }
        //public ClotheStyleType ClotheStyle { get; set; }
        //public BodyArtType BodyArt { get; set; }

        //public string Tags { get; set; }
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

    [JsonConverter(typeof(StringEnumConverter))]
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
    }

    #endregion
}
