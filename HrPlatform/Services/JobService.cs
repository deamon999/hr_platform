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

    public async Task<PaginationResult<Job>> GetPagedAsync(
        int pageNumber = 1, int pageSize = 10,
        string filter = "all", string sortBy = "date",
        HrPlatform.Data.Enums.CdlClass? cdlClass = null,
        decimal? minPay = null,
        HrPlatform.Data.Enums.TrailerType? trailerType = null,
        bool matchProfileOnly = false,
        DriverProfile? driverProfile = null,
        int? companyId = null)
    {
        var query = db.Jobs
            .Include(j => j.Applications)
            .Include(j => j.Company)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(j => j.CompanyId == companyId.Value);
        }

        // Existing active/inactive filter
        query = filter switch
        {
            "active" => query.Where(j => j.IsActive),
            "inactive" => query.Where(j => !j.IsActive),
            _ => query
        };

        // NEW: CDL class — show if job has no requirement OR matches driver
        if (cdlClass.HasValue)
            query = query.Where(j =>
                j.RequiredCdlClass == null ||
                j.RequiredCdlClass == cdlClass.Value);

        // NEW: Minimum pay
        if (minPay.HasValue)
            query = query.Where(j =>
                j.PayRate == null || j.PayRate >= minPay.Value);

        // NEW: Trailer type
        if (trailerType.HasValue)
            query = query.Where(j =>
                j.RequiredTrailerType == null ||
                j.RequiredTrailerType == trailerType.Value);

        // NEW: 'Jobs for me' — filter by driver's own profile
        if (matchProfileOnly && driverProfile != null)
        {
            var driverClass = driverProfile.License?.Class;

            query = query.Where(j =>
                (j.RequiredCdlClass == null || j.RequiredCdlClass == driverClass) &&
                (j.MinYearsExperience == 0 ||
                    j.MinYearsExperience <= driverProfile.YearsOfExperience));
        }

        query = sortBy switch
        {
            "pay" => query.OrderByDescending(j => j.PayRate),
            _ => query.OrderByDescending(j => j.PostedAt)
        };

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