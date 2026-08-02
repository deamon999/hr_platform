using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class InvitationService : IInvitationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJobInvitationService _jobInvitationService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public InvitationService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IJobInvitationService jobInvitationService,
        IEmailService emailService, ISmsService smsService)
    {
        _db = db;
        _userManager = userManager;
        _jobInvitationService = jobInvitationService;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<List<Invitation>> GetRecentInvitationsAsync(int count = 20)
    {
        return await _db.Invitations
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<PaginationResult<Invitation>> GetRecentInvitationsPagedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var invitations = await _db.Invitations
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invitations.Paginate(pageNumber, pageSize);
    }

    public async Task<Invitation> CreateAsync(Invitation invitation)
    {
        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync();
        return invitation;
    }

    public async Task<bool> InvitationExistsAsync(string email, string phone)
    {
        return await _db.Invitations.AnyAsync(i =>
            (i.Email == email || i.Phone == phone) &&
            !i.IsUsed &&
            i.ExpiresAt > DateTime.UtcNow);
    }


    public Task<Invitation?> GetValidAsync(string token) =>
        _db.Invitations.FirstOrDefaultAsync(i =>
            i.Token == token &&
            !i.IsUsed &&
            i.ExpiresAt > DateTime.UtcNow);

    public async Task MarkUsedAsync(string token)
    {
        var inv = await _db.Invitations.FirstOrDefaultAsync(i => i.Token == token);
        if (inv is not null)
        {
            inv.IsUsed = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsUsedByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return;
        var emailLower = email.Trim().ToLowerInvariant();
        var invs = await _db.Invitations
            .Where(i => i.Email != null && i.Email.ToLower() == emailLower && !i.IsUsed)
            .ToListAsync();
            
        foreach (var inv in invs)
        {
            inv.IsUsed = true;
        }
        
        if (invs.Any())
        {
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> PendingExistsAsync(string email) =>
        _db.Invitations.AnyAsync(i =>
            i.Email == email.Trim().ToLowerInvariant() &&
            !i.IsUsed &&
            i.ExpiresAt > DateTime.UtcNow);

    public async Task<InviteResult> InviteAsync(Invitation invitation, Uri link)
    {
        // ── Resolve existing user ────────────────────────────
        ApplicationUser? existingUser = !string.IsNullOrWhiteSpace(invitation.Email)
            ? await _userManager.FindByEmailAsync(invitation.Email)
            : await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == invitation.Phone);

        bool isDriver = existingUser is not null &&
                        (await _userManager.IsInRoleAsync(existingUser, "Driver"));

        if (existingUser is not null && isDriver)
        {
            var job = await _db.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == invitation.JobId);

            if (job is null)
                return InviteResult.Fail("Job not found.");
            // Guard: already invited to this job
            bool alreadyInvited = await _db.JobInvitations
                .AnyAsync(ji => ji.UserId == existingUser.Id && ji.JobId == invitation.JobId);

            if (alreadyInvited)
                return InviteResult.Fail("Driver already has an invitation for this job.");

            // Create JobInvitation directly
            await _jobInvitationService.CreateFromRegistrationAsync(existingUser.Id, invitation.JobId!.Value);

            return InviteResult.Ok(existing: true);
        }
        else
        {
            // ── New user — create regular Invitation with JobId ──
            var hasEmail = !string.IsNullOrWhiteSpace(invitation.Email); // for phone-only: caller should supply email separately
            // or handle SMS — left as extension point

            // Guard: pending invitation already exists
            bool pendingExists = await _db.Invitations
                .AnyAsync(i => (hasEmail ? i.Email == invitation.Email : i.Phone == invitation.Phone)
                               && i.Role == invitation.Role && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);

            if (pendingExists)
                return InviteResult.Fail("A pending invitation already exists for this email.");

            if (ContactMethod.Email == invitation.ContactMethod)
            {
                await _emailService.SendEmailAsync(
                    invitation.Email,
                    null,
                    "You've been invited to join",
                    BuildRegistrationInviteEmail(link, invitation.ExpiresAt));
            }
            else
            {
                await _smsService.SendDriverInviteAsync(invitation.Phone, null, null, link.AbsolutePath);
            }

            return InviteResult.Ok(existing: false);
        }
    }

    public async Task<InviteResult> ResendAsync(int invitationId, Uri baseUri)
    {
        var original = await _db.Invitations.FindAsync(invitationId);
        if (original is null)
            return InviteResult.Fail("Invitation not found.");

        // Mark original as used so it won't show as pending
        original.IsUsed = true;
        await _db.SaveChangesAsync();

        // Create a fresh invitation with new token + 7-day expiry
        var fresh = new Invitation
        {
            ContactMethod = original.ContactMethod,
            Email = original.Email,
            Phone = original.Phone,
            Role = original.Role,
            CompanyId = original.CompanyId,
            JobId = original.JobId
            // Token and ExpiresAt are set by the entity defaults
        };

        var link = new Uri(baseUri,
            $"/Account/Register?token={fresh.Token}");

        var result = await InviteAsync(fresh, link);

        if (result.Success)
            await CreateAsync(fresh);

        return result;
    }

    private static string BuildRegistrationInviteEmail(Uri link, DateTime expiresAt) => $"""
                                                                                         <p>Hello,</p>
                                                                                         <p>You have been invited to join the platform.</p>
                                                                                         <p>Please register first using the link below:</p>
                                                                                         <p>
                                                                                           <a href="{link}" style="
                                                                                              display:inline-block;padding:10px 20px;
                                                                                              background:#0d6efd;color:#fff;
                                                                                              text-decoration:none;border-radius:4px;">
                                                                                             Register &amp; View Invitation
                                                                                           </a>
                                                                                         </p>
                                                                                         <p style="color:#6c757d;font-size:.85em;">
                                                                                         <p>This link expires on <strong>{expiresAt:MMMM dd, yyyy}</strong>.</p>
                                                                                         </p>
                                                                                         """;

    public async Task<bool> DeleteAsync(int invitationId)
    {
        var invitation = await _db.Invitations.FindAsync(invitationId);
        if (invitation == null)
            return false;

        _db.Invitations.Remove(invitation);
        await _db.SaveChangesAsync();
        return true;
    }
}