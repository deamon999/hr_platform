using HrPlatform.Data.Models;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IJobService
{
    Task<List<Job>> GetAllAsync();
    Task<PaginationResult<Job>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string filter = "all", string sortBy = "date");
    Task<Job?> GetByIdAsync(int id);
    Task<Job> CreateAsync(Job job);
    Task UpdateAsync(Job job);
    Task SetActiveAsync(int id, bool active);
    Task DeleteAsync(int id);
    Task<List<Company>> GetCompaniesAsync();
}