using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface IJobService
{
    Task<List<Job>> GetAllAsync();
    Task<Job?> GetByIdAsync(int id);
    Task<Job> CreateAsync(Job job);
    Task UpdateAsync(Job job);
    Task SetActiveAsync(int id, bool active);
    Task DeleteAsync(int id);
    Task<List<Company>> GetCompaniesAsync();
}