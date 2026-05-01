using HrPlatform.Models;

namespace HrPlatform.Data.Entities;

public class Invitation
{
    public int Id { get; set; }
    public ContactMethod ContactMethod { get; set; }

    /// <summary>Unique URL-safe token included in the invitation link.</summary>
    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Email address the invitation was sent to.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number the invitation was sent to.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Role to assign on registration: "Manager" or "Driver".</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Company identifier — only populated for Manager invitations.</summary>
    public int? CompanyId { get; set; }

    /// <summary>Job identifier — only populated for Driver invitations.</summary>
    public int? JobId { get; set; }

    /// <summary>Whether the invitation has already been consumed.</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Whether the invitation is waiting for job application.</summary>
    public bool IsJobPending { get; set; }

    /// <summary>Invitation link expires after 7 days by default.</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}