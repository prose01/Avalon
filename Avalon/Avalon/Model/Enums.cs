using System.Runtime.Serialization;

namespace Avalon.Model
{
    public enum AllergiesType  // Boolean
    {
        Yes,
        No
    }

    public enum ClotheStyleType
    {
        NotChosen,
        Casual,
        Dressy,
        Dandy,
        Stylish,
        Formal,
        Other

    }

    public enum BodyType
    {
        NotChosen,
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
        Vegetarian
    }

    public enum EducationLevelType
    {
        Bachelor,
        Master,
        Phd,
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
    public enum EmploymentLevelType
    {
    }

    public enum EmploymentAreaType
    {
    }

    public enum EmploymentStatusType
    {
        Unemployed,
        Employed,
        SelfEmployed,
        Other
    }

    public enum LocationType
    {
        City,
        Suburb,
        Countryside,
        Other
    }

    public enum PoliticalOrientationType
    {
    }

    public enum ReligiousOrientationType
    {
    }

    public enum SexualOrientationType
    {
        Heterosexual,
        Homosexual,
        Bisexual,
        Asexual
    }

    public enum SmokingHabitsType
    {
        [EnumMember(Value = "Non Smoker")]
        NonSmoker,
        OccasionalSmoker,
        Smoker
    }

    public enum SportType
    {
    }

    public enum MaritalStatusType
    {
        Single,
        Married,
        Divorced,
        Other
    }
}
