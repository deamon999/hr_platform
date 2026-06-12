using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IDriverProfileService
{
    Task<DriverProfile?> GetByUserIdAsync(string userId);
    Task<DriverProfile?> GetByIdAsync(int id);
    Task<PaginationResult<DriverProfile>> GetAllPagedAsync(ProfileSearch profileSearch, int pageNumber = 1, int pageSize = 10);
    Task<PaginationResult<DriverProfile>> GetByCompanyPagedAsync(ProfileSearch profileSearch, int companyId, int pageNumber = 1, int pageSize = 10);
    Task<DriverProfile> CreateAsync(DriverProfile profile);
    Task UpdateAsync(DriverProfile profile, string currentUserId);
    Task DeleteAsync(int id, string currentUserId, bool isAdmin);
}