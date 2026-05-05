using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class JobInvitationService : IJobInvitationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly NavigationManager _nav;

    public JobInvitationService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        NavigationManager nav,
        ISmsService smsService)
    {
        _db = db;
        _userManager = userManager;
        _emailService = emailService;
        _nav = nav;
        _smsService = smsService;
    }

    // ──────────────────────────────────────────────────────────
    // Called from Register.razor after user is created
    // ──────────────────────────────────────────────────────────

    public async Task CreateFromRegistrationAsync(string userId, int jobId)
    {
        var jobInvitation = new JobInvitation
        {
            UserId = userId,
            JobId = jobId
        };
        _db.JobInvitations.Add(jobInvitation);
        await _db.SaveChangesAsync();

        // Notify driver that their job invitation is ready
        var user = await _userManager.FindByIdAsync(userId);
        var job = await _db.Jobs.FindAsync(jobId);

        if (user?.Email is not null && job is not null)
        {
            var userName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim();

            await _emailService.SendEmailAsync(
                user.Email,
                string.IsNullOrWhiteSpace(userName) ? null : userName,
                $"Your job invitation is ready: {job.Title}",
                BuildJobInviteEmail(job, _nav.ToAbsoluteUri("/driver/invitations"))
            );
        }
    }

    // ──────────────────────────────────────────────────────────
    // Queries
    // ──────────────────────────────────────────────────────────

    public async Task<List<JobInvitation>> GetForDriverAsync(string userId)
    {
        return await _db.JobInvitations
            .Include(ji => ji.Job)
            .Where(ji => ji.UserId == userId)
            .OrderByDescending(ji => ji.CreatedAt)
            .ToListAsync();
    }

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

    public async Task<bool> UpdateStatusAsync(
        int invitationId, string driverUserId, JobInvitationStatus status)
    {
        var invitation = await _db.JobInvitations
            .FirstOrDefaultAsync(ji =>
                ji.Id == invitationId && ji.UserId == driverUserId);

        if (invitation is null) return false;

        invitation.Status = status;
        invitation.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ──────────────────────────────────────────────────────────
    // Email templates
    // ──────────────────────────────────────────────────────────

    private static string BuildJobInviteEmail(Job job, Uri link) => $"""
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
}