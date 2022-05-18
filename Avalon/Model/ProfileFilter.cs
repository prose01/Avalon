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
        public BodyType Body { get; set; }
        public SmokingHabitsType SmokingHabits { get; set; }
        public HasChildrenType HasChildren { get; set; }
        public WantChildrenType WantChildren { get; set; }
        public HasPetsType HasPets { get; set; }
        public LivesInType LivesIn { get; set; }
        public EducationType Education { get; set; }
        public EducationStatusType EducationStatus { get; set; }
        public EmploymentStatusType EmploymentStatus { get; set; }
        public SportsActivityType SportsActivity { get; set; }
        public EatingHabitsType EatingHabits { get; set; }
        public ClotheStyleType ClotheStyle { get; set; }
        public BodyArtType BodyArt { get; set; }
    }
}