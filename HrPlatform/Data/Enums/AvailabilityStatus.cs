using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum AvailabilityStatus
{
    [Description("Actively Looking")] ActivelyLooking,
    [Description("Open To Opportunities")] OpenToOpportunities,
    [Description("Not Available")] NotAvailable
}