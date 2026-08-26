using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum PayPeriod
{
    [Description("Hourly")]
    Hour,
    
    [Description("Per Mile")]
    Mile,
    
    [Description("Weekly")]
    Week,
    
    [Description("Annually")]
    Year,
    
    [Description("Monthly")]
    Month,
    
    [Description("Percentage of Load")]
    PercentageOfLoad
}