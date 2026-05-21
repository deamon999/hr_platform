using HrPlatform.Data.Models;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IDriverProfileService
{
    Task<DriverProfile?> GetByUserIdAsync(string userId);
    Task<DriverProfile?> GetByIdAsync(int id);
    Task<List<DriverProfile>> GetAllAsync();
    Task<PaginationResult<DriverProfile>> GetAllPagedAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<DriverProfile>> GetByCompanyAsync(int companyId);
    Task<PaginationResult<DriverProfile>> GetByCompanyPagedAsync(int companyId, int pageNumber = 1, int pageSize = 10);
    Task<DriverProfile> CreateAsync(DriverProfile profile);
    Task UpdateAsync(DriverProfile profile, string currentUserId);
    Task DeleteAsync(int id, string currentUserId, bool isAdmin);
}