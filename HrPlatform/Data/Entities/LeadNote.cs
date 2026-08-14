using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Models;

namespace HrPlatform.Data.Entities;

public class LeadNote
{
    public int Id { get; set; }

    public int LeadId { get; set; }
    public Lead? Lead { get; set; }

    [Required]
    public string AuthorUserId { get; set; } = default!;
    public ApplicationUser? AuthorUser { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = default!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsEdited { get; set; }
}
