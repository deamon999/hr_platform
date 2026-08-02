using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum ViolationType
{
    [Description("Moving Violation")]
    MovingViolation,
    
    [Description("At-Fault Accident")]
    AtFaultAccident,
    
    [Description("Not At-Fault Accident")]
    NotAtFaultAccident,
    
    [Description("DUI / DWI")]
    DUI,
    
    [Description("Other")]
    Other
}
