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
            .ThenInclude(l => l!.Endorsements)
            .Include(p => p.User)
            .ThenInclude(u => u!.Applications);

        if (profileSearch.Availability.HasValue)
            q = q.Where(p => p.AvailabilityStatus == profileSearch.Availability.Value);
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
            .ThenInclude(l => l!.Endorsements)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory)
            .ThenInclude(e => e!.TrailerTypes)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<DriverProfile?> GetByIdAsync(int id)
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .ThenInclude(l => l!.Endorsements)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory)
            .ThenInclude(e => e!.TrailerTypes)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .Include(p => p.Documents)
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
        queryable = queryable.Where(p => p.User != null && p.User.Applications.Any(a => a.Job != null && a.Job.CompanyId == companyId));
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
        var existing = await db.DriverProfiles.AsNoTracking()
            .Include(p => p.License).ThenInclude(l => l!.Endorsements)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory).ThenInclude(e => e!.TrailerTypes)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == profile.Id);

        if (existing is null)
            throw new InvalidOperationException("Profile not found");
        if (existing.UserId != currentUserId)
            throw new UnauthorizedAccessException("Only the profile owner may update the profile.");

        // Prune empty UI entries before tracking updates
        foreach (var emp in profile.EmploymentHistory.Where(e => string.IsNullOrWhiteSpace(e.CompanyName)).ToList()) {
            profile.EmploymentHistory.Remove(emp);
            db.Entry(emp).State = EntityState.Detached;
        }

        // Find and explicitly delete orphaned items
        var incomingJobs = profile.EmploymentHistory.Select(e => e.Id).ToHashSet();
        foreach (var job in existing.EmploymentHistory) {
            if (!incomingJobs.Contains(job.Id)) db.Entry(job).State = EntityState.Deleted;
            else {
                var incJob = profile.EmploymentHistory.First(e => e.Id == job.Id);
                var incTrailers = incJob.TrailerTypes.Select(t => t.Id).ToHashSet();
                foreach (var tt in job.TrailerTypes) {
                    if (!incTrailers.Contains(tt.Id)) db.Entry(tt).State = EntityState.Deleted;
                }
            }
        }
        
        var incomingSkills = profile.Skills.Select(e => e.Id).ToHashSet();
        foreach (var skill in existing.Skills) {
            if (!incomingSkills.Contains(skill.Id)) db.Entry(skill).State = EntityState.Deleted;
        }

        var incomingDocs = profile.Documents.Select(e => e.Id).ToHashSet();
        foreach (var doc in existing.Documents) {
            if (!incomingDocs.Contains(doc.Id)) db.Entry(doc).State = EntityState.Deleted;
        }

        if (existing.License != null && profile.License == null) {
            db.Entry(existing.License).State = EntityState.Deleted;
        } else if (existing.License != null && profile.License != null) {
            var incomingEnds = profile.License.Endorsements.Select(e => e.Id).ToHashSet();
            foreach (var end in existing.License.Endorsements) {
                if (!incomingEnds.Contains(end.Id)) db.Entry(end).State = EntityState.Deleted;
            }
        }

        if (existing.MedicalCard != null && profile.MedicalCard == null) db.Entry(existing.MedicalCard).State = EntityState.Deleted;

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