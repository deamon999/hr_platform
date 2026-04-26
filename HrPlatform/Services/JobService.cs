using HrPlatform.Data;
using HrPlatform.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class JobService(ApplicationDbContext db) : IJobService
{
    public async Task<List<Job>> GetAllAsync()
    {
        return await db.Jobs
            .Include(j => j.Applications)
            .Include(j => j.Company)
            .OrderByDescending(j => j.PostedAt)
            .ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await db.Jobs
            .Include(j => j.Applications)
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<Job> CreateAsync(Job job)
    {
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    public async Task UpdateAsync(Job job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        db.Jobs.Update(job);
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool active)
    {
        var job = await db.Jobs.FindAsync(id)
                  ?? throw new KeyNotFoundException($"Job {id} not found");
        job.IsActive = active;
        job.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var job = await db.Jobs.FindAsync(id);
        if (job is null)
            return;
        db.Jobs.Remove(job);
        await db.SaveChangesAsync();
    }

    public async Task<List<Company>> GetCompaniesAsync()
    {
        return await db.Companies.OrderBy(c => c.Name).ToListAsync();
    }
}