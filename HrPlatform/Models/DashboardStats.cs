namespace HrPlatform.Models;

public record DashboardStats(
    int TotalPipeline,
    int TotalJobs,
    int OpenJobs,
    int ApplicationsThisWeek,
    int PendingApplications,
    int AcceptedThisMonth,
    int InvitationsPending,
    int ExpiringLicenses,
    int RegisteredProfiles,
    int ActionableLeads,
    Dictionary<string, int> ApplicationsByStatus);