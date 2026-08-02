using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum AvailabilityStatus
{
    [Description("Ready To Start Immediately")] ReadyToStartImmediately,
    [Description("Actively Looking")] ActivelyLooking,
    [Description("2 - Week Notice")] TwoWeekNotice,
    [Description("Not Available")] NotAvailable
}