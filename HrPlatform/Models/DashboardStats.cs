namespace HrPlatform.Models;

public record DashboardStats(
    int TotalDrivers,
    int TotalJobs,
    int OpenJobs,
    int ApplicationsThisWeek,
    int PendingApplications,
    int AcceptedThisMonth,
    int InvitationsPending,
    int ExpiringLicenses,
    int ActiveLeads,
    Dictionary<string, int> ApplicationsByStatus);