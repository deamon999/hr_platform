using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IJobInvitationService
{
    /// <summary>
    /// Invites a driver to a job.
    /// - If the email/phone belongs to an existing Driver → creates JobInvitation directly.
    /// - If not registered → creates a regular Invitation (Driver role + JobId).
    /// Sends appropriate email in both cases.
    /// </summary>
    // Task<InviteDriverResult> InviteDriverAsync(string emailOrPhone, int jobId);

    /// <summary>
    /// Called during registration when a regular Invitation carried a JobId.
    /// Creates the JobInvitation for the newly registered user.
    /// </summary>
    Task CreateFromRegistrationAsync(string userId, int jobId);

    /// <summary>
    /// Returns all job invitations for a specific driver (for /driver/invitations).
    /// </summary>
    // Task<List<JobInvitation>> GetForDriverAsync(string userId);

    /// <summary>
    /// Returns all job invitations across all jobs belonging to a company
    /// (for /manager/job-invitations). Optionally filtered by a single job.
    /// </summary>
    // Task<List<JobInvitation>> GetForCompanyAsync(string companyId, int? jobId = null);

    /// <summary>Updates the status of a job invitation (Accept / Decline).</summary>
    // Task<bool> UpdateStatusAsync(int invitationId, string userId, JobInvitationStatus status);
}