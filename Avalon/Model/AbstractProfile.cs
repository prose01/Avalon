﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Avalon.Model
{
    [BsonKnownTypes(typeof(CurrentUser), typeof(Profile))]
    public abstract class AbstractProfile
    {
        #region special properties
        [MaxLength(10)]
        public abstract Dictionary<string, DateTime> Visited { get; set; }
        [MaxLength(10)]
        public abstract Dictionary<string, DateTime> IsBookmarked { get; set; }
        #endregion

        [BsonId]
        public abstract ObjectId _id { get; set; }
        public abstract string Auth0Id { get; set; }
        public abstract string ProfileId { get; set; }
        public abstract bool Admin { get; set; }
        public abstract string Name { get; set; }

        [DataType(DataType.DateTime)]
        public abstract DateTime CreatedOn { get; set; }

        [DataType(DataType.DateTime)]
        public abstract DateTime UpdatedOn { get; set; }

        [DataType(DataType.DateTime)]
        public abstract DateTime LastActive { get; set; }

        [Range(16, 120)]
        public abstract int? Age { get; set; }

        [Range(0, 250)]
        public abstract int? Height { get; set; }

        public abstract bool Contactable { get; set; }

        [StringLength(2000, ErrorMessage = "Description length cannot be more than 2000.")]
        public abstract string Description { get; set; }
        public abstract List<ImageModel> Images { get; set; }

        [MaxLength(10)]
        public abstract List<string> Tags { get; set; }

        //public abstract string JobTitle { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract GenderType Gender { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract SexualOrientationType SexualOrientation { get; set; } // TODO: Should this be encrypted?

        [BsonRepresentation(BsonType.String)]
        public abstract BodyType Body { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract SmokingHabitsType SmokingHabits { get; set; }

        //[BsonRepresentation(BsonType.String)]  // Maybe not
        //public abstract AllergiesType Allergies { get; set; }

        public abstract HasChildrenType HasChildren { get; set; }

        public abstract WantChildrenType WantChildren { get; set; }

        public abstract HasPetsType HasPets { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract LivesInType LivesIn { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract EducationType Education { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract EducationStatusType EducationStatus { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract EmploymentStatusType EmploymentStatus { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EmploymentAreaType EmploymentArea { get; set; }

        //[BsonRepresentation(BsonType.String)]
        //public abstract EmploymentLevelType EmploymentLevel { get; set; }

        //[BsonRepresentation(BsonType.String)] //Maybe not
        //public abstract PoliticalOrientationType PoliticalOrientation { get; set; }

        //[BsonRepresentation(BsonType.String)] //Maybe not
        //public abstract ReligiousOrientationType ReligiousOrientation { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract SportsActivityType SportsActivity { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract EatingHabitsType EatingHabits { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract ClotheStyleType ClotheStyle { get; set; }

        [BsonRepresentation(BsonType.String)]
        public abstract BodyArtType BodyArt { get; set; }
    }
}
