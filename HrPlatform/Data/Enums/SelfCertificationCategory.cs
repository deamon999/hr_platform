using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum SelfCertificationCategory
{
    [Description("Non-Excepted Interstate (NI)")]
    NonExceptedInterstate = 1,

    [Description("Excepted Interstate (EI)")]
    ExceptedInterstate = 2,

    [Description("Non-Excepted Intrastate (NA)")]
    NonExceptedIntrastate = 3,

    [Description("Excepted Intrastate (EA)")]
    ExceptedIntrastate = 4
}
