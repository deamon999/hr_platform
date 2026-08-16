using HrPlatform.Data.Entities;
using HrPlatform.Models;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface ILeadService
{
    Task<PaginationResult<Lead>> GetLeadsPagedAsync(int pageNumber, int pageSize, int? companyId = null, string? searchTerm = null, LeadStatus? status = null, string? addedByUserId = null, bool actionableOnly = false);
    Task<Lead?> GetByIdAsync(int id);
    Task<Lead> CreateAsync(Lead lead);
    Task UpdateAsync(Lead lead);
    Task DeleteAsync(int id);
    Task<List<Lead>> GetActionableRemindersAsync(int? companyId);
    Task<List<ApplicationUser>> GetUsersWithLeadsAsync(int? companyId);
    Task<HashSet<string>> GetRegisteredEmailsAsync(IEnumerable<string> emails);
}
