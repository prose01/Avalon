using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Avalon.Model
{
    public class ProfileFilter
    {
        [StringLength(50, ErrorMessage = "Name length cannot be more than 50.")]
        public string Name { get; set; }

        public List<int?> Age { get; set; }
        public List<int?> Height { get; set; }

        [StringLength(2000, ErrorMessage = "Description length cannot be more than 2000.")]
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public GenderType? Gender { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<BodyType> Body { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<SmokingHabitsType> SmokingHabits { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<HasChildrenType> HasChildren { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<WantChildrenType> WantChildren { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<HasPetsType> HasPets { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<LivesInType> LivesIn { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<EducationType> Education { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<EducationStatusType> EducationStatus { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<EmploymentStatusType> EmploymentStatus { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<SportsActivityType> SportsActivity { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<EatingHabitsType> EatingHabits { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<ClotheStyleType> ClotheStyle { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public List<BodyArtType> BodyArt { get; set; }
    }
}