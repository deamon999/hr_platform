using HrPlatform.Data.Models;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface ICompanyService
{
    Task<List<Company>> GetAllAsync();
    Task<PaginationResult<Company>> GetPagedAsync(int pageNumber = 1, int pageSize = 10);
    Task<Company?> GetByIdAsync(int id);
    Task<Company> CreateAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(int id);
}