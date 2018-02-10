using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

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
        public string Body { get; set; } = string.Empty;


        //public int Height { get; set; }

        //public int Weight { get; set; }

        //public string Kropsbygning_Conformation { get; set; }

        //public string Smoking { get; set; }

        //public string PetAllergies { get; set; }

        //public string Children { get; set; }

        //Ønsker børn

        //Seneste aktivitet

        //UDDANNELSESOMRÅDE

        //UDDANNELSESTITEL

        //UDDANNELSESNIVEAU

        //UDDANNELSESSTATUS

        //public string EmploymentStatus { get; set; }

        //public string EmploymentArea { get; set; }

        //public string JobTitle { get; set; }

        //STILLINGSNIVEAU

        //public string PoliticalOrientation { get; set; }

        //public string Sport { get; set; }

        //public string EatingHabits { get; set; }

        //public string Style { get; set; }

        //public string Tags { get; set; }
    }
}
