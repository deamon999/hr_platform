using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum SelfCertificationCategory
{
    [Description("Non-Excepted Interstate")]
    NonExceptedInterstate = 1,

    [Description("Excepted Interstate")]
    ExceptedInterstate = 2,

    [Description("Non-Excepted Intrastate")]
    NonExceptedIntrastate = 3,

    [Description("Excepted Intrastate")]
    ExceptedIntrastate = 4
}
