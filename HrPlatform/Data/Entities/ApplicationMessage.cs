using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Models;

namespace HrPlatform.Data.Entities;

public class ApplicationMessage
{
    public int Id { get; set; }
    
    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = default!;
    
    [Required]
    public string SenderId { get; set; } = default!;
    public ApplicationUser Sender { get; set; } = default!;
    
    [Required]
    public string Content { get; set; } = default!;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRead { get; set; } = false;
}
