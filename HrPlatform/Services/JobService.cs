using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class JobService(ApplicationDbContext db) : IJobService
{
    private IQueryable<Job> GetBaseQuery()
    {
        return db.Jobs
            .Include(j => j.Applications)
            .Include(j => j.Company)
            .OrderByDescending(j => j.PostedAt);
    }

    public async Task<List<Job>> GetAllAsync()
    {
        return await GetBaseQuery().ToListAsync();
    }

    public async Task<PaginationResult<Job>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string filter = "all", string sortBy = "date")
    {
        var query = db.Jobs
            .Include(j => j.Applications)
            .Include(j => j.Company)
            .AsQueryable();

        // 1. Apply Filter
        query = filter switch
        {
            "active" => query.Where(j => j.IsActive),
            "inactive" => query.Where(j => !j.IsActive),
            _ => query
        };

        // 2. Apply Sort (EF Core safely translates navigation property null checks)
        query = sortBy switch
        {
            "title" => query.OrderBy(j => j.Title),
            "company" => query.OrderBy(j => j.Company.Name),
            _ => query.OrderByDescending(j => j.PostedAt)
        };

        // Execute query and paginate
        var jobs = await query.ToListAsync();
        return jobs.Paginate(pageNumber, pageSize);
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