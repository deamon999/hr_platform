using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class JobApplicationService(ApplicationDbContext db) : IJobApplicationService
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
            .Include(a => a.Job)
            .ThenInclude(j => j.Company) // Included to ensure company name renders in UI
            .AsQueryable();

        // 1. Filter for Manager
        if (isManager)
        {
            if (companyId.HasValue)
            {
                // Let EF handle the join automatically!
                q = q.Where(a => a.Job.CompanyId == companyId.Value);
            }
            else
            {
                // Safety catch: If manager has no company, return empty list
                return new List<JobApplication>();
            }
        }
        // 2. Filter for Driver
        else if (isDriver)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                q = q.Where(a => a.UserId == userId);
            }
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
}