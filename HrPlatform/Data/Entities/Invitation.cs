using HrPlatform.Data.Models;
using HrPlatform.Models;

namespace HrPlatform.Data.Entities;

public class Invitation
{
    public int Id { get; set; }
    public ContactMethod ContactMethod { get; set; }

    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public int? CompanyId { get; set; }
    public int? JobId { get; set; }

    // 1. ADD THESE NAVIGATION PROPERTIES
    public Company? Company { get; set; }
    public Job? Job { get; set; }

    public bool IsUsed { get; set; } = false;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}