using System;
using System.Collections.Generic;

namespace Avalon.Model
{
    public class ProfileFilter
    {
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime LastActive { get; set; }
        public List<int?> Age { get; set; }
        public List<int?> Height { get; set; }
        public List<int?> Weight { get; set; }
        public string Description { get; set; }

        //public override string[] Tags { get; set; }

        //public override string JobTitle { get; set; } = string.Empty;

        public GenderType Gender { get; set; }

        public BodyType Body { get; set; }

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
