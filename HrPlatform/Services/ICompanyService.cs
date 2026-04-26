using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface ICompanyService
{
    Task<List<Company>> GetAllAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<Company> CreateAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(int id);
}