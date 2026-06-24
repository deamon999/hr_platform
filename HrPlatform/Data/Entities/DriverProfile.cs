using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Entities;

namespace HrPlatform.Data.Models;

public class DriverProfile
{
    public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;

    // Add the navigation property
    public ApplicationUser? User { get; set; }

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

    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.OpenToOpportunities;

    public DateOnly? AvailableFrom { get; set; }

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

    // Navigation property for skills junction table
    public ICollection<DriverProfileSkill> Skills { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverEmployment> EmploymentHistory { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverEducation> EducationHistory { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverCertification> Certifications { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverViolation> ViolationHistory { get; set; } = [];

    public IEnumerable<TrailerType> AllTrailerTypes =>
        EmploymentHistory.SelectMany(e => e.TrailerTypes.Select(t => t.TrailerType)).Distinct();

    /// <summary>
    /// Returns a 0-100 completeness score based on the 7 most
    /// important hiring criteria for CDL drivers.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CompletenessScore
    {
        get
        {
            var checks = new[]
            {
                !string.IsNullOrWhiteSpace(FirstName) &&
                !string.IsNullOrWhiteSpace(LastName) &&
                !string.IsNullOrWhiteSpace(PhoneNumber),

                License != null &&
                License.ExpiryDate > DateOnly.FromDateTime(DateTime.Today),

                MedicalCard != null &&
                MedicalCard.ExpiryDate > DateOnly.FromDateTime(DateTime.Today),

                EmploymentHistory.Any(),

                YearsOfExperience > 0 && TotalMilesDriven > 0,

                !string.IsNullOrWhiteSpace(ProfessionalSummary),

                Certifications.Any() || Skills.Any()
            };
            return checks.Count(c => c) * 100 / checks.Length;
        }
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string CompletenessLabel => CompletenessScore switch
    {
        >= 85 => "Complete",
        >= 60 => "Good",
        >= 40 => "Needs work",
        _ => "Incomplete"
    };

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string CompletenessBadgeClass => CompletenessScore switch
    {
        >= 85 => "bg-success",
        >= 60 => "bg-info text-dark",
        >= 40 => "bg-warning text-dark",
        _ => "bg-danger"
    };

    [NotMapped]
    public List<string> MissingFields
    {
        get
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(PhoneNumber))
                missing.Add("Basic Contact Info");
            
            if (License == null || License.ExpiryDate <= DateOnly.FromDateTime(DateTime.Today))
                missing.Add("Valid CDL License");

            if (MedicalCard == null || MedicalCard.ExpiryDate <= DateOnly.FromDateTime(DateTime.Today))
                missing.Add("Valid DOT Medical Card");

            if (!EmploymentHistory.Any())
                missing.Add("Employment History");

            if (YearsOfExperience <= 0 || TotalMilesDriven <= 0)
                missing.Add("Driving Experience (Years & Miles)");

            if (string.IsNullOrWhiteSpace(ProfessionalSummary))
                missing.Add("Professional Summary");

            if (!Certifications.Any() && !Skills.Any())
                missing.Add("Skills or Certifications");

            return missing;
        }
    }

    // Helper methods for managing skills
    public bool HasSkill(string skill) =>
        Skills.Any(s => s.Skill.Equals(skill, StringComparison.OrdinalIgnoreCase));

    public void AddSkill(string skill)
    {
        if (!string.IsNullOrWhiteSpace(skill) && !HasSkill(skill))
        {
            Skills.Add(new DriverProfileSkill { Skill = skill.Trim() });
        }
    }

    public void RemoveSkill(string skill)
    {
        var existing = Skills.FirstOrDefault(s => s.Skill.Equals(skill, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            Skills.Remove(existing);
        }
    }

    public IEnumerable<string> GetSkillValues() =>
        Skills.Select(s => s.Skill);
}