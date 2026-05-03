using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class JobInvitationService : IJobInvitationService
{
     private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;          // your existing service
        private readonly NavigationManager _nav;

        public JobInvitationService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            NavigationManager nav)
        {
            _db          = db;
            _userManager = userManager;
            _emailService = emailService;
            _nav          = nav;
        }

        // ──────────────────────────────────────────────────────────
        // Invite driver to job
        // ──────────────────────────────────────────────────────────

        // public async Task<InviteDriverResult> InviteDriverAsync(string emailOrPhone, int jobId)
        // {
        //     var job = await _db.Jobs
        //         .Include(j => j.Company)
        //         .FirstOrDefaultAsync(j => j.Id == jobId);
        //
        //     if (job is null)
        //         return InviteDriverResult.Fail("Job not found.");
        //
        //     // ── Resolve existing user ────────────────────────────
        //     ApplicationUser? existingUser = emailOrPhone.Contains('@')
        //         ? await _userManager.FindByEmailAsync(emailOrPhone)
        //         : await _userManager.Users
        //               .FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhone);
        //
        //     bool isDriver = existingUser is not null &&
        //                     (await _userManager.IsInRoleAsync(existingUser, "Driver"));
        //
        //     if (existingUser is not null && isDriver)
        //     {
        //         // Guard: already invited to this job
        //         bool alreadyInvited = await _db.JobInvitations
        //             .AnyAsync(ji => ji.UserId == existingUser.Id && ji.JobId == jobId);
        //
        //         if (alreadyInvited)
        //             return InviteDriverResult.Fail("Driver already has an invitation for this job.");
        //
        //         // Create JobInvitation directly
        //         var jobInvitation = new JobInvitation
        //         {
        //             UserId = existingUser.Id,
        //             JobId  = jobId
        //         };
        //         _db.JobInvitations.Add(jobInvitation);
        //         await _db.SaveChangesAsync();
        //
        //         // Send notification email
        //         await _emailService.SendEmailAsync(
        //             existingUser.Email!,
        //             $"You've been invited to: {job.Title}",
        //             BuildJobInviteEmail(existingUser.Email!, job, _nav.ToAbsoluteUri("/driver/invitations")));
        //
        //         return InviteDriverResult.Ok(existing: true);
        //     }
        //     else
        //     {
        //         // ── New user — create regular Invitation with JobId ──
        //         var email = emailOrPhone.Contains('@')
        //             ? emailOrPhone
        //             : emailOrPhone; // for phone-only: caller should supply email separately
        //                             // or handle SMS — left as extension point
        //
        //         // Guard: pending invitation already exists
        //         bool pendingExists = await _db.Invitations
        //             .AnyAsync(i => i.Email == email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);
        //
        //         if (pendingExists)
        //             return InviteDriverResult.Fail("A pending invitation already exists for this email.");
        //
        //         var invitation = new Invitation
        //         {
        //             Email  = email.Trim().ToLowerInvariant(),
        //             Role   = "Driver",
        //             JobId  = jobId
        //         };
        //         _db.Invitations.Add(invitation);
        //         await _db.SaveChangesAsync();
        //
        //         var registerLink = _nav.ToAbsoluteUri($"/Account/Register?token={invitation.Token}");
        //
        //         await _emailService.SendEmailAsync(
        //             email,
        //             $"You've been invited to join and apply for: {job.Title}",
        //             BuildRegistrationInviteEmail(email, job, registerLink.ToString()));
        //
        //         return InviteDriverResult.Ok(existing: false);
        //     }
        // }

        // ──────────────────────────────────────────────────────────
        // Called from Register.razor after user is created
        // ──────────────────────────────────────────────────────────

        public async Task CreateFromRegistrationAsync(string userId, int jobId)
        {
            var jobInvitation = new JobInvitation
            {
                UserId = userId,
                JobId  = jobId
            };
            _db.JobInvitations.Add(jobInvitation);
            await _db.SaveChangesAsync();

            // Notify driver that their job invitation is ready
            var user = await _userManager.FindByIdAsync(userId);
            var job  = await _db.Jobs.FindAsync(jobId);

            if (user?.Email is not null && job is not null)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    user.FirstName + " " + user.LastName,
                    $"Your job invitation is ready: {job.Title}",
                    BuildJobInviteEmail(user.Email, job,
                        _nav.ToAbsoluteUri("/driver/invitations")));
            }
        }

        // ──────────────────────────────────────────────────────────
        // Queries
        // ──────────────────────────────────────────────────────────

        // public Task<List<JobInvitation>> GetForDriverAsync(string userId) =>
        //     _db.JobInvitations
        //        .Include(ji => ji.Job)
        //        .Where(ji => ji.UserId == userId)
        //        .OrderByDescending(ji => ji.CreatedAt)
        //        .ToListAsync();
        //
        // public async Task<List<JobInvitation>> GetForCompanyAsync(
        //     string companyId, Guid? jobId = null)
        // {
        //     // Step 1: load company job ids
        //     var jobIds = await _db.Jobs
        //         .Where(j => j.CompanyId == companyId)
        //         .Select(j => j.Id)
        //         .ToListAsync();
        //
        //     if (!jobIds.Any())
        //         return new List<JobInvitation>();
        //
        //     // Step 2: filter invitations by those ids (optional single-job filter)
        //     var query = _db.JobInvitations
        //         .Include(ji => ji.Job)
        //         .Include(ji => ji.User)
        //         .Where(ji => jobIds.Contains(ji.JobId));
        //
        //     if (jobId.HasValue)
        //         query = query.Where(ji => ji.JobId == jobId.Value);
        //
        //     return await query
        //         .OrderByDescending(ji => ji.CreatedAt)
        //         .ToListAsync();
        // }

        // ──────────────────────────────────────────────────────────
        // Status update (driver accept / decline)
        // ──────────────────────────────────────────────────────────

        // public async Task<bool> UpdateStatusAsync(
        //     Guid invitationId, string driverUserId, JobInvitationStatus status)
        // {
        //     var invitation = await _db.JobInvitations
        //         .FirstOrDefaultAsync(ji =>
        //             ji.Id == invitationId && ji.UserId == driverUserId);
        //
        //     if (invitation is null) return false;
        //
        //     invitation.Status     = status;
        //     invitation.ReviewedAt = DateTime.UtcNow;
        //     await _db.SaveChangesAsync();
        //     return true;
        // }

        // ──────────────────────────────────────────────────────────
        // Email templates
        // ──────────────────────────────────────────────────────────

        private static string BuildJobInviteEmail(string email, Job job, Uri link) => $"""
            <p>Hello,</p>
            <p>You have been invited to apply for the position of
               <strong>{job.Title}</strong> at <strong>{job.Company?.Name}</strong>.</p>
            <p>
              <a href="{link}" style="
                 display:inline-block;padding:10px 20px;
                 background:#0d6efd;color:#fff;
                 text-decoration:none;border-radius:4px;">
                View Invitation
              </a>
            </p>
            """;

        private static string BuildRegistrationInviteEmail(string email, Job job, string link) => $"""
            <p>Hello,</p>
            <p>You have been invited to join the platform and apply for
               <strong>{job.Title}</strong> at <strong>{job.Company?.Name}</strong>.</p>
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
              This link expires in 7 days.
            </p>
            """;
}