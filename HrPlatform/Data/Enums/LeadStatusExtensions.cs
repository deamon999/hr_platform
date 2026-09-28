using System.Linq.Expressions;
using HrPlatform.Data.Entities;

namespace HrPlatform.Data.Enums;

/// <summary>
/// Domain extension methods for LeadStatus lifecycle and state checks.
/// </summary>
public static class LeadStatusExtensions
{
    /// <summary>
    /// Checks if a lead status is a terminal/closed state (Hired, NotInterested, Rejected, NotQualified).
    /// </summary>
    public static bool IsTerminal(this LeadStatus status) =>
        status is LeadStatus.Hired or LeadStatus.NotInterested or LeadStatus.Rejected or LeadStatus.NotQualified;

    /// <summary>
    /// Checks if a lead status is actionable/active (not New and not in a terminal state).
    /// </summary>
    public static bool IsActionable(this LeadStatus status) =>
        !status.IsTerminal() && status != LeadStatus.New;

    /// <summary>
    /// Checks if a lead status is a legacy status kept for historical database compatibility.
    /// </summary>
    public static bool IsLegacy(this LeadStatus status) =>
        status is LeadStatus.Converted or LeadStatus.Contacted or LeadStatus.Invited;
}

/// <summary>
/// EF Core LINQ query expressions for lead filtering to ensure consistency across services.
/// </summary>
public static class LeadExpressions
{
    /// <summary>
    /// Expression predicate to filter actionable leads (in progress, neither New nor closed/terminal).
    /// </summary>
    public static readonly Expression<Func<Lead, bool>> IsActionable = l =>
        l.Status != LeadStatus.New &&
        l.Status != LeadStatus.Hired &&
        l.Status != LeadStatus.NotInterested &&
        l.Status != LeadStatus.Rejected &&
        l.Status != LeadStatus.NotQualified;

    /// <summary>
    /// Expression predicate to filter non-terminal leads.
    /// </summary>
    public static readonly Expression<Func<Lead, bool>> IsNotTerminal = l =>
        l.Status != LeadStatus.Hired &&
        l.Status != LeadStatus.NotInterested &&
        l.Status != LeadStatus.Rejected &&
        l.Status != LeadStatus.NotQualified;
}
