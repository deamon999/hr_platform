using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class Job
{
    public int Id { get; set; }

    [Required] [MaxLength(150)] public string Title { get; set; } = default!;

    // link to Company
    [Range(1, int.MaxValue, ErrorMessage = "Please select a company")]
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    [MaxLength(100)] public string? City { get; set; }

    [MaxLength(50)] public string? State { get; set; }

    [MaxLength(20)] public string? ZipCode { get; set; }

    [MaxLength(4000)] public string? Description { get; set; }

    public decimal? PayRate { get; set; }
    public PayPeriod? PayPeriod { get; set; }

    [MaxLength(100)] public string? PayNotes { get; set; }

    public CdlClass? RequiredCdlClass { get; set; }
    public List<CdlEndorsement> RequiredEndorsements { get; set; } = [];
    public TrailerType? RequiredTrailerType { get; set; }
    public int MinYearsExperience { get; set; }

    public RouteType? RouteType { get; set; }
    public decimal? SignOnBonus { get; set; }
    public bool RequiresManualTransmission { get; set; }
    public bool AllowsPets { get; set; }
    public bool AllowsRiders { get; set; }
    public bool IsTeamDriving { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsConfidential { get; set; } = false;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public EmploymentType? EmploymentType { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<JobApplication> Applications { get; set; } = [];
}