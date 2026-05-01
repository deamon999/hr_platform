using HrPlatform.Data.Entities;

namespace HrPlatform.Services;

public interface IInvitationService
{
    Task<List<Invitation>> GetRecentInvitationsAsync(int count);

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
}