using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IAdminUserService
{
    Task<List<UserViewModel>> GetAllUsersWithRolesAsync();
    Task<PaginationResult<UserViewModel>> GetAllUsersWithRolesPagedAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<UserViewModel>> GetUsersByCompanyWithRolesAsync(int companyId);
    Task<PaginationResult<UserViewModel>> GetUsersByCompanyWithRolesPagedAsync(int companyId, int pageNumber = 1, int pageSize = 10);
    Task<UserViewModel> GetUserByIdAsync(string id);
    Task DeleteAsync(string id);
}