using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IDriverProfileService
{
    Task<DriverProfile?> GetByUserIdAsync(string userId);
    Task<DriverProfile?> GetByIdAsync(int id);
    Task<List<DriverProfile>> GetAllAsync(AvailabilityStatus? status);
    Task<PaginationResult<DriverProfile>> GetAllPagedAsync(AvailabilityStatus? status, int pageNumber = 1, int pageSize = 10);
    Task<List<DriverProfile>> GetByCompanyAsync(int companyId, AvailabilityStatus? status);
    Task<PaginationResult<DriverProfile>> GetByCompanyPagedAsync(AvailabilityStatus? status, int companyId, int pageNumber = 1, int pageSize = 10);
    Task<DriverProfile> CreateAsync(DriverProfile profile);
    Task UpdateAsync(DriverProfile profile, string currentUserId);
    Task DeleteAsync(int id, string currentUserId, bool isAdmin);
}