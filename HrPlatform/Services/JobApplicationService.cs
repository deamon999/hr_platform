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
            .Include(a => a.DriverProfile)
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<JobApplication>> GetAllAsync(string? sortBy = null)
    {
        var q = db.JobApplications
            .Include(a => a.DriverProfile)
            .Include(a => a.Job)
            .AsQueryable();

        return sortBy switch
        {
            "status" => await q.OrderBy(a => a.Status).ThenByDescending(a => a.AppliedAt).ToListAsync(),
            "driver" => await q.OrderBy(a => a.DriverProfile.LastName).ThenBy(a => a.DriverProfile.FirstName)
                .ToListAsync(),
            "job" => await q.OrderBy(a => a.Job.Title).ThenByDescending(a => a.AppliedAt).ToListAsync(),
            _ => await q.OrderByDescending(a => a.AppliedAt).ToListAsync()
        };
    }

    public async Task<List<JobApplication>> GetByJobAsync(int jobId)
    {
        return await db.JobApplications
            .Include(a => a.DriverProfile)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByDriverAsync(int driverProfileId)
    {
        return await db.JobApplications
            .Include(a => a.Job)
            .Where(a => a.DriverProfileId == driverProfileId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<bool> HasAppliedAsync(int jobId, int driverProfileId)
    {
        return await db.JobApplications
            .AnyAsync(a => a.JobId == jobId && a.DriverProfileId == driverProfileId);
    }

    public async Task<JobApplication> ApplyAsync(int jobId, int driverProfileId)
    {
        var application = new JobApplication { JobId = jobId, DriverProfileId = driverProfileId };
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
}