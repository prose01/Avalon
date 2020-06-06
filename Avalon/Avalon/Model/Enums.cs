using System.Runtime.Serialization;

namespace Avalon.Model
{
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
}
