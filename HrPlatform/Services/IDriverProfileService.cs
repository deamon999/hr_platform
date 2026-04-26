using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface IDriverProfileService
{
    Task<DriverProfile?> GetByUserIdAsync(string userId);
    Task<DriverProfile?> GetByIdAsync(int id);
    Task<List<DriverProfile>> GetAllAsync();
    Task<List<DriverProfile>> GetByCompanyAsync(int companyId);
    Task<DriverProfile> CreateAsync(DriverProfile profile);
    Task UpdateAsync(DriverProfile profile, string currentUserId);
    Task DeleteAsync(int id, string currentUserId, bool isAdmin);
}