using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Data.Models;

public class Company
{
    public int Id { get; set; }

    [Required] [MaxLength(200)] public string Name { get; set; } = default!;

    [MaxLength(100)] public string? RegistrationNumber { get; set; }

    public DateOnly? DateRegistered { get; set; }

    [MaxLength(200)] public string? Address { get; set; }

    [MaxLength(100)] public string? City { get; set; }

    [MaxLength(50)] public string? State { get; set; }

    [MaxLength(100)] public string? Country { get; set; }

    [Phone] [MaxLength(50)] public string? ContactPhone { get; set; }

    [EmailAddress] [MaxLength(150)] public string? ContactEmail { get; set; }

    // Driver-facing fields
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? FleetSize { get; set; }
    public HrPlatform.Data.Enums.HomeTimeFrequency? HomeTime { get; set; }
    public bool PaidCdlTraining { get; set; }
    public bool SignOnBonus { get; set; }
    public bool BenefitsOffered { get; set; }
    public string? BenefitsSummary { get; set; }
    public bool HiringNewGrads { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<ApplicationUser> Users { get; set; } = [];
}