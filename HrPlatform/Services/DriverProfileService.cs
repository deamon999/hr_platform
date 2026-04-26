using HrPlatform.Data;
using HrPlatform.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class DriverProfileService(ApplicationDbContext db) : IDriverProfileService
{
    public async Task<DriverProfile?> GetByUserIdAsync(string userId)
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory)
            .Include(p => p.EducationHistory)
            .Include(p => p.Certifications)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<DriverProfile?> GetByIdAsync(int id)
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .Include(p => p.MedicalCard)
            .Include(p => p.EmploymentHistory)
            .Include(p => p.EducationHistory)
            .Include(p => p.Certifications)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<DriverProfile>> GetAllAsync()
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .Include(p => p.Applications)
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<List<DriverProfile>> GetByCompanyAsync(int companyId)
    {
        return await db.DriverProfiles
            .Include(p => p.License)
            .Include(p => p.Applications)
            .Where(p => p.Applications.Any(a => a.Job.CompanyId == companyId))
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .ToListAsync();
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