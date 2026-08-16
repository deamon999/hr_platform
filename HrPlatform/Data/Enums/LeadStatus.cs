using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum LeadStatus
{
    [Description("New")]
    New,
    [Description("Not Interested")]
    NotInterested,
    [Description("Ready To Start")]
    ReadyToStart,
    [Description("Attempt Contact")]
    AttemptContact,
    [Description("Needs to be Contacted")]
    NeedsToBeContacted,
    [Description("Ready in 2 Weeks")]
    ReadyIn2Weeks,
    [Description("Hired")]
    Hired,

    // Legacy statuses to prevent EF Core crashes on existing data
    [Description("Converted")]
    Converted,

    // Legacy statuses to prevent EF Core crashes on existing data
    [Description("Contacted")]
    Contacted,
    [Description("Rejected")]
    Rejected,
    [Description("Invited")]
    Invited
}
