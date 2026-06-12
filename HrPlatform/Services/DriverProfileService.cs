using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class DriverProfileService(ApplicationDbContext db) : IDriverProfileService
{
    private IOrderedQueryable<DriverProfile> GetBaseQuery(ProfileSearch profileSearch)
    {
        IQueryable<DriverProfile> q = db.DriverProfiles
            .Include(p => p.License)
            .ThenInclude(l => l.Endorsements)
            .Include(p => p.User)
            .ThenInclude(u => u.Applications);

        // Availability status
        if (profileSearch.Availability.HasValue)
        {
            q = q.Where(p => p.AvailabilityStatus == profileSearch.Availability.Value);
        }

        // Name search
        if (!string.IsNullOrWhiteSpace(profileSearch.Name))
            q = q.Where(p =>
                p.FirstName.Contains(profileSearch.Name) ||
                p.LastName.Contains(profileSearch.Name) ||
                (p.FirstName + " " + p.LastName).Contains(profileSearch.Name));

        // CDL class filter
        if (profileSearch.CdlClass.HasValue)
            q = q.Where(p => p.License != null &&
                             p.License.Class == profileSearch.CdlClass.Value);

        // CDL endorsement filter
        if (profileSearch.RequiredEndorsement.HasValue)
            q = q.Where(p => p.License != null &&
                             p.License.Endorsements.Any(e => e.Endorsement == profileSearch.RequiredEndorsement.Value));

        // Minimum years experience
        if (profileSearch.MinYears.HasValue)
            q = q.Where(p => p.YearsOfExperience >= profileSearch.MinYears.Value);

        return q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);
    }

    public async Task<DriverProfile?> GetByUserIdAsync(string userId)
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .ThenInclude(l => l.Endorsements)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory)
            .ThenInclude(e => e.TrailerTypes)
            .Include(p => p.EducationHistory)
            .Include(p => p.Certifications)
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<DriverProfile?> GetByIdAsync(int id)
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .ThenInclude(l => l.Endorsements)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory)
            .ThenInclude(e => e.TrailerTypes)
            .Include(p => p.EducationHistory)
            .Include(p => p.Certifications)
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PaginationResult<DriverProfile>> GetAllPagedAsync(ProfileSearch profileSearch, int pageNumber = 1, int pageSize = 10)
    {
        return await GetBaseQuery(profileSearch).PaginateAsync(pageNumber, pageSize);
    }

    public async Task<PaginationResult<DriverProfile>> GetByCompanyPagedAsync(ProfileSearch profileSearch, int companyId, int pageNumber = 1,
        int pageSize = 10)
    {
        IQueryable<DriverProfile> queryable = GetBaseQuery(profileSearch);
        queryable = queryable.Where(p => p.User.Applications.Any(a => a.Job.CompanyId == companyId));
        return await queryable.PaginateAsync(pageNumber, pageSize);
    }

    public async Task<DriverProfile> CreateAsync(DriverProfile profile)
    {
        db.DriverProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateAsync(DriverProfile profile, string currentUserId)
    {
        // ensure only owner can update
        var existing = await db.DriverProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == profile.Id);
        if (existing is null)
            throw new InvalidOperationException("Profile not found");
        if (existing.UserId != currentUserId)
            throw new UnauthorizedAccessException("Only the profile owner may update the profile.");

        profile.UpdatedAt = DateTime.UtcNow;
        db.DriverProfiles.Update(profile);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string currentUserId, bool isAdmin)
    {
        var profile = await db.DriverProfiles.FindAsync(id);
        if (profile is null)
            return;

        if (!isAdmin && profile.UserId != currentUserId)
            throw new UnauthorizedAccessException("Only the profile owner or an administrator may delete the profile.");

        db.DriverProfiles.Remove(profile);
        await db.SaveChangesAsync();
    }
}