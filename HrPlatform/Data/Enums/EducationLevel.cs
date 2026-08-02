using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum EducationLevel
{
    [Description("Some High School")]
    SomeHighSchool,
    [Description("High School Diploma")]
    HighSchoolDiploma,
    [Description("GED")]
    GED,
    [Description("Some College")]
    SomeCollege,
    [Description("Associate Degree")]
    AssociateDegree,
    [Description("Bachelor's Degree")]
    BachelorDegree,
    [Description("Vocational Certificate")]
    VocationalCertificate,
    [Description("Other")]
    Other
}