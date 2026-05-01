using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IAdminUserService
{
    Task<List<UserViewModel>> GetAllUsersWithRolesAsync();
    Task<UserViewModel> GetUserByIdAsync(string id);
    Task DeleteAsync(string id);
}