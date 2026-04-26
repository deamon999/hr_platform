using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverProfile
{
    public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;

    [Required] [MaxLength(100)] public string FirstName { get; set; } = default!;

    [MaxLength(100)] public string? MiddleName { get; set; }

    [Required] [MaxLength(100)] public string LastName { get; set; } = default!;

    [Required] public DateOnly DateOfBirth { get; set; }

    [Required] [Phone] [MaxLength(20)] public string PhoneNumber { get; set; } = default!;

    [MaxLength(20)] public string? AlternatePhone { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = default!;

    [MaxLength(300)] public string? LinkedInUrl { get; set; }

    [MaxLength(200)] public string? StreetAddress { get; set; }

    [MaxLength(100)] public string? City { get; set; }

    [MaxLength(50)] public string? State { get; set; }

    [MaxLength(20)] public string? ZipCode { get; set; }

    [MaxLength(2000)] public string? ProfessionalSummary { get; set; }

    public int YearsOfExperience { get; set; }
    public long TotalMilesDriven { get; set; }
    public long AccidentFreeMiles { get; set; }
    public int StatesOperated { get; set; }
    public int AverageWeeklyMiles { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ValidateComplexType] public DriverLicense? License { get; set; }

    [ValidateComplexType] public DriverMedicalCard? MedicalCard { get; set; }

    public List<string> Skills { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverEmployment> EmploymentHistory { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverEducation> EducationHistory { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverCertification> Certifications { get; set; } = [];

    public ICollection<JobApplication> Applications { get; set; } = [];

    public IEnumerable<TrailerType> AllTrailerTypes =>
        EmploymentHistory.SelectMany(e => e.TrailerTypes).Distinct();
}