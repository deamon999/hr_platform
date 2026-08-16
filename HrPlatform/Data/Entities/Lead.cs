using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Data;

namespace HrPlatform.Data.Entities;

public class Lead
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = default!;

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? AddedByUserId { get; set; }
    public ApplicationUser? AddedByUser { get; set; }

    public List<TrailerType> TrailerTypes { get; set; } = new();
    public DateTime? ReminderDate { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastContactedAt { get; set; }

    public string? ConvertedUserId { get; set; }
    public ApplicationUser? ConvertedUser { get; set; }

    public ICollection<LeadNote> Notes { get; set; } = new List<LeadNote>();
}
