using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;

namespace HrPlatform.Data.Entities;

public class JobInvitation
{
    public int Id { get; set; }

    // ── Driver ───────────────────────────────────────────────
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    // ── Job ──────────────────────────────────────────────────
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    // ── Status ───────────────────────────────────────────────
    public JobInvitationStatus Status { get; set; } = JobInvitationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}