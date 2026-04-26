using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface IJobApplicationService
{
    Task<JobApplication?> GetAsync(int id);
    Task<List<JobApplication>> GetAllAsync(string? sortBy = null);
    Task<List<JobApplication>> GetByJobAsync(int jobId);
    Task<List<JobApplication>> GetByDriverAsync(int driverProfileId);
    Task<bool> HasAppliedAsync(int jobId, int driverProfileId);
    Task<JobApplication> ApplyAsync(int jobId, int driverProfileId);
    Task ReviewAsync(int id, ApplicationStatus status, string? notes);
}