using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IDashboardService
{
    Task<DashboardStats> GetAdminStatsAsync();
    Task<DashboardStats> GetManagerStatsAsync(int companyId);
}