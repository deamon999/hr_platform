using HrPlatform.Data.Entities;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface ILeadService
{
    Task<PaginationResult<Lead>> GetLeadsPagedAsync(int pageNumber, int pageSize, int? companyId = null, string? searchTerm = null);
    Task<Lead?> GetByIdAsync(int id);
    Task<Lead> CreateAsync(Lead lead);
    Task UpdateAsync(Lead lead);
    Task DeleteAsync(int id);
}
