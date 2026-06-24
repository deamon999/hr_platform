using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class JobApplicationService(
    ApplicationDbContext db,
    IEmailService emailService,
    ISmsService smsService) : IJobApplicationService
{
    public async Task<JobApplication?> GetAsync(int id)
    {
        return await db.JobApplications
            .Include(a => a.User)
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<JobApplication>> GetAllAsync(string? sortBy = null)
    {
        var q = db.JobApplications
            .Include(a => a.User)
            .Include(a => a.Job)
            .AsQueryable();

        return sortBy switch
        {
            "status" => await q.OrderBy(a => a.Status).ThenByDescending(a => a.AppliedAt).ToListAsync(),
            "driver" => await q.OrderBy(a => a.User.LastName).ThenBy(a => a.User.FirstName)
                .ToListAsync(),
            "job" => await q.OrderBy(a => a.Job.Title).ThenByDescending(a => a.AppliedAt).ToListAsync(),
            _ => await q.OrderByDescending(a => a.AppliedAt).ToListAsync()
        };
    }

    public async Task<PaginationResult<JobApplication>> GetAllPagedAsync(int pageNumber = 1, int pageSize = 10, string? sortBy = null)
    {
        var apps = await GetAllAsync(sortBy);
        return apps.Paginate(pageNumber, pageSize);
    }

    public async Task<List<JobApplication>> GetByJobAsync(int jobId)
    {
        return await db.JobApplications
            .Include(a => a.User)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByUserAsync(string applicationUserId)
    {
        return await db.JobApplications
            .Include(a => a.Job)
            .Where(a => a.UserId == applicationUserId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<bool> HasAppliedAsync(int jobId, string applicationUserId)
    {
        return await db.JobApplications
            .AnyAsync(a => a.JobId == jobId && a.UserId == applicationUserId);
    }

    public async Task<JobApplication> ApplyAsync(int jobId, string applicationUserId)
    {
        var application = new JobApplication { JobId = jobId, UserId = applicationUserId };
        db.JobApplications.Add(application);
        await db.SaveChangesAsync();
        return application;
    }

    public async Task ReviewAsync(int id, ApplicationStatus status, string? notes)
    {
        var app = await db.JobApplications.FindAsync(id)
                  ?? throw new KeyNotFoundException($"Application {id} not found");
        app.Status = status;
        app.ReviewedAt = DateTime.UtcNow;
        app.ReviewerNotes = notes;
        await db.SaveChangesAsync();

        await NotifyDriverAsync(app, status, notes);
    }

    public async Task WithdrawAsync(int id)
    {
        var app = await db.JobApplications.FindAsync(id)
                  ?? throw new KeyNotFoundException($"Application {id} not found");

        db.JobApplications.Remove(app);
        await db.SaveChangesAsync();
    }

    public async Task<List<JobApplication>> GetAllFilteredAsync(
        string? userId,
        bool isManager,
        bool isDriver,
        int? companyId,
        string? sortBy = null)
    {
        var q = db.JobApplications
            .Include(a => a.User)
            .ThenInclude(u => u!.DriverProfile)
            .ThenInclude(p => p!.License)
            .Include(a => a.User)
            .ThenInclude(u => u!.DriverProfile)
            .ThenInclude(p => p!.EmploymentHistory)
            .Include(a => a.Job)
            .ThenInclude(j => j!.Company)
            .AsQueryable();

        // 1. Filter for Manager
        if (isManager)
        {
            if (companyId.HasValue)
                // Let EF handle the join automatically!
                q = q.Where(a => a.Job.CompanyId == companyId.Value);
            else
                // Safety catch: If manager has no company, return empty list
                return new List<JobApplication>();
        }
        // 2. Filter for Driver
        else if (isDriver)
        {
            if (!string.IsNullOrEmpty(userId)) q = q.Where(a => a.UserId == userId);
        }

        // Note: If the user is an Admin (neither Driver nor Manager), 
        // no filters are applied, and they see everything.

        return sortBy switch
        {
            "status" => await q.OrderBy(a => a.Status).ThenByDescending(a => a.AppliedAt).ToListAsync(),
            "driver" => await q.OrderBy(a => a.User.LastName).ThenBy(a => a.User.FirstName).ToListAsync(),
            "job" => await q.OrderBy(a => a.Job.Title).ThenByDescending(a => a.AppliedAt).ToListAsync(),
            _ => await q.OrderByDescending(a => a.AppliedAt).ToListAsync()
        };
    }

    public async Task<PaginationResult<JobApplication>> GetAllFilteredPagedAsync(
        string? userId,
        bool isManager,
        bool isDriver,
        int? companyId,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null)
    {
        var apps = await GetAllFilteredAsync(userId, isManager, isDriver, companyId, sortBy);
        return apps.Paginate(pageNumber, pageSize);
    }

    private async Task NotifyDriverAsync(
        JobApplication app, ApplicationStatus status, string? notes)
    {
        var driver = app.User;
        if (driver is null) return;

        var jobTitle = app.Job?.Title ?? "the position";
        var company = app.Job?.Company?.Name ?? "the company";
        var driverName = $"{driver.FirstName} {driver.LastName}".Trim();

        var subject = status switch
        {
            ApplicationStatus.Accepted =>
                $"Congratulations! Your application for {jobTitle} was accepted",
            ApplicationStatus.Rejected =>
                $"Your application for {jobTitle} has been reviewed",
            ApplicationStatus.UnderReview =>
                $"Your application for {jobTitle} is now under review",
            _ => $"Update on your application for {jobTitle}"
        };

        if (!string.IsNullOrWhiteSpace(driver.Email))
        {
            await emailService.SendEmailAsync(
                driver.Email,
                driverName,
                subject,
                BuildStatusEmail(jobTitle, company, status, notes));
        }

        // SMS fallback / supplement
        else if (!string.IsNullOrWhiteSpace(driver.PhoneNumber))
        {
            var smsText = status switch
            {
                ApplicationStatus.Accepted =>
                    $"Good news! Your application for {jobTitle} at {company}" +
                    " was accepted. Log in for details.",
                ApplicationStatus.Rejected =>
                    $"Your application for {jobTitle} at {company}" +
                    " has been reviewed. Log in to see feedback.",
                _ => $"Your application for {jobTitle} at {company}" +
                     " has been updated. Log in to check your status."
            };
            await smsService.SendDriverInviteAsync(
                driver.PhoneNumber, driverName, string.Empty, smsText);
        }
    }

    private static string BuildStatusEmail(
        string job, string company,
        ApplicationStatus status, string? notes)
    {
        return $"""
                <p>Hello,</p>
                <p>Your application for <strong>{job}</strong>
                at <strong>{company}</strong> has been updated.</p>
                <p><strong>New status:</strong> {status}</p>
                {(!string.IsNullOrEmpty(notes)
                    ? $"<p><em>Note from recruiter: {notes}</em></p>"
                    : "")}
                <p><a href=\"#\" style=\"background:#0d6efd;color:#fff;
                   padding:10px 20px;text-decoration:none;
                   border-radius:4px;\">View My Applications</a></p>
                """;
    }
}