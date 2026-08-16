using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class DashboardService(ApplicationDbContext db) : IDashboardService
{
    public async Task<DashboardStats> GetAdminStatsAsync()
    {
        var now = DateTime.UtcNow;
        var week = now.AddDays(-7);

        // Fixed: Explicitly declare this as UTC
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var in30 = DateOnly.FromDateTime(now.AddDays(30));

        var byStatus = await db.JobApplications
            .GroupBy(a => a.Status.ToString())
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        return new DashboardStats(
            TotalDrivers: await db.DriverProfiles.CountAsync(),
            TotalJobs: await db.Jobs.CountAsync(),
            OpenJobs: await db.Jobs.CountAsync(j => j.IsActive),
            ApplicationsThisWeek: await db.JobApplications
                .CountAsync(a => a.AppliedAt >= week),
            PendingApplications: await db.JobApplications
                .CountAsync(a => a.Status == ApplicationStatus.Pending),
            AcceptedThisMonth: await db.JobApplications
                .CountAsync(a => a.Status == ApplicationStatus.Accepted
                                 && a.ReviewedAt >= monthStart),
            InvitationsPending: await db.Invitations
                .CountAsync(i => !i.IsUsed && i.ExpiresAt > now),
            ExpiringLicenses: await db.DriverLicenses
                .CountAsync(l => l.ExpiryDate <= in30
                                 && l.ExpiryDate >= DateOnly.FromDateTime(now)),
            ActiveLeads: await db.Leads
                .CountAsync(l => l.Status != LeadStatus.Hired && l.Status != LeadStatus.NotInterested && l.Status != LeadStatus.Rejected),
            ApplicationsByStatus: byStatus);
    }

    public async Task<DashboardStats> GetManagerStatsAsync(int companyId)
    {
        var now = DateTime.UtcNow;
        var week = now.AddDays(-7);

        // Fixed: Explicitly declare this as UTC
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var in30 = DateOnly.FromDateTime(now.AddDays(30));

        var companyApps = db.JobApplications
            .Where(a => a.Job.CompanyId == companyId);

        var byStatus = await companyApps
            .GroupBy(a => a.Status.ToString())
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        // Expiring licenses in driver pool (applied to this company)
        var driverIds = await companyApps.Select(a => a.UserId)
            .Distinct().ToListAsync();

        int expiringInPool = await db.DriverLicenses
            .Where(l => driverIds.Contains(l.DriverProfile.UserId)
                        && l.ExpiryDate <= in30
                        && l.ExpiryDate >= DateOnly.FromDateTime(now))
            .CountAsync();

        return new DashboardStats(
            TotalDrivers: driverIds.Count,
            TotalJobs: await db.Jobs.CountAsync(j => j.CompanyId == companyId),
            OpenJobs: await db.Jobs.CountAsync(j => j.CompanyId == companyId && j.IsActive),
            ApplicationsThisWeek: await companyApps.CountAsync(a => a.AppliedAt >= week),
            PendingApplications: await companyApps
                .CountAsync(a => a.Status == ApplicationStatus.Pending),
            AcceptedThisMonth: await companyApps
                .CountAsync(a => a.Status == ApplicationStatus.Accepted
                                 && a.ReviewedAt >= monthStart),
            InvitationsPending: await db.Invitations
                .CountAsync(i => i.CompanyId == companyId
                                 && !i.IsUsed && i.ExpiresAt > now),
            ExpiringLicenses: expiringInPool,
            ActiveLeads: await db.Leads
                .CountAsync(l => l.CompanyId == companyId && l.Status != LeadStatus.Hired && l.Status != LeadStatus.NotInterested && l.Status != LeadStatus.Rejected),
            ApplicationsByStatus: byStatus);
    }
}