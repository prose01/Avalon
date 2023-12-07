namespace Avalon.Model
{
    public enum ClotheStyleType
    {
        Casual,
        Dressy,
        Dandy,
        Stylish,
        Formal,
        Other
    }

    public enum BodyType
    {
        Atlethic,
        Chubby,
        Normal,
        Robust,
        Slim,
        Other
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
        Vegetarian,
        Other
    }

    public enum EducationStatusType
    {
        Graduated,
        Student,
        Other
    }

    public enum EducationType
    {
        School,
        Highschool,
        University,
        Other
    }

    public enum EmploymentStatusType
    {
        Unemployed,
        Employed,
        SelfEmployed,
        Other
    }

    public enum HasChildrenType
    {
        Yes,
        No,
        Other
    }

    public enum WantChildrenType
    {
        Yes,
        No,
        Other
    }

    public enum HasPetsType
    {
        Yes,
        No,
        Other
    }

    public enum LivesInType
    {
        City,
        Suburb,
        Countryside,
        Other
    }

    public enum SmokingHabitsType
    {
        NonSmoker,
        OccasionalSmoker,
        Smoker,
        Other
    }

    public enum SportsActivityType
    {
        Regularly,
        SomeRegularity,
        Seldom,
        Never,
        Other
    }


    // Enum types used for other stuff than Profiles.
    public enum OrderByType
    {
        CreatedOn,
        UpdatedOn,
        LastActive,
        Name
    }
}
