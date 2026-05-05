using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface IJobApplicationService
{
    Task<JobApplication?> GetAsync(int id);
    Task<List<JobApplication>> GetAllAsync(string? sortBy = null);
    Task<List<JobApplication>> GetByJobAsync(int jobId);
    Task<List<JobApplication>> GetByUserAsync(string applicationUserId);
    Task<bool> HasAppliedAsync(int jobId, string applicationUserId);
    Task<JobApplication> ApplyAsync(int jobId, string applicationUserId);
    Task ReviewAsync(int id, ApplicationStatus status, string? notes);
    Task WithdrawAsync(int id);
    Task<List<JobApplication>> GetAllFilteredAsync(string? userId, bool isManager, bool isDriver, int? companyId, string? sortBy = null);
}