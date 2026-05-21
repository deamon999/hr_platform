using HrPlatform.Data.Entities;
using HrPlatform.Models;

namespace HrPlatform.Services;

public interface IInvitationService
{
    Task<List<Invitation>> GetRecentInvitationsAsync(int count);
    Task<PaginationResult<Invitation>> GetRecentInvitationsPagedAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>Creates and persists a new invitation record.</summary>
    Task<Invitation> CreateAsync(Invitation invitation);

    Task<bool> InvitationExistsAsync(string email, string phone);

    /// <summary>
    /// Returns the invitation for the given token only if it is
    /// valid (not used, not expired). Returns null otherwise.
    /// </summary>
    Task<Invitation?> GetValidAsync(string token);

    /// <summary>Marks the invitation as consumed so it cannot be reused.</summary>
    Task MarkUsedAsync(string token);

    /// <summary>
    /// Returns true when a still-valid (pending, not expired) invitation
    /// already exists for the given email address.
    /// </summary>
    Task<bool> PendingExistsAsync(string email);

    /// <summary>
    /// Invites a driver to a job.
    /// - If the email/phone belongs to an existing Driver → creates JobInvitation directly.
    /// - If not registered → creates a regular Invitation (Driver role + JobId).
    /// Sends appropriate email in both cases.
    /// </summary>
    Task<InviteResult> InviteAsync(Invitation invitation, Uri link);
}