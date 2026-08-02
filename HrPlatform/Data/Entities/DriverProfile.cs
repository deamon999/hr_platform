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

    public int LastWizardStep { get; set; } = 0;
    
    public bool IsApplicationCompleted { get; set; } = false;

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

    [MaxLength(200)] public string? StreetAddress { get; set; }
    
    [MaxLength(100)] public string? AddressLine2 { get; set; }

    [MaxLength(100)] public string? City { get; set; }

    [MaxLength(50)] public string? State { get; set; }

    [MaxLength(20)] public string? ZipCode { get; set; }

    // Trucking Lifestyle & Preferences
    public HomeTimeFrequency? PreferredHomeTime { get; set; }
    public string? PreferredPosition { get; set; }
    public List<string>? PreferredFreight { get; set; } = new();
    public List<string>? PreferredRegions { get; set; } = new();
    public string? MinimumWeeklyPay { get; set; }
    
    public bool CanDriveManual { get; set; }
    public bool WantsToDriveWithPets { get; set; }
    public bool WantsToDriveWithRiders { get; set; }
    public bool WantsTeamDriving { get; set; }

    public int YearsOfExperience { get; set; }
    public long TotalMilesDriven { get; set; }
    public long AccidentFreeMiles { get; set; }
    public int StatesOperated { get; set; }
    public int AverageWeeklyMiles { get; set; }

    // Military Service
    public bool HasMilitaryService { get; set; }
    public string? MilitaryBranch { get; set; }
    public int? MilitaryYears { get; set; }

    // Consents & Authorizations
    public bool ConsentFCRA { get; set; }
    public bool ConsentPSP { get; set; }
    public bool ConsentMVR { get; set; }
    public bool ConsentClearinghouse { get; set; }
    public bool ConsentEmployment { get; set; }
    public string? ElectronicSignatureName { get; set; }
    public DateOnly? SignatureDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ValidateComplexType] public DriverLicense? License { get; set; }

    [ValidateComplexType] public DriverMedicalCard? MedicalCard { get; set; }

    // Navigation property for skills junction table
    public ICollection<DriverProfileSkill> Skills { get; set; } = [];

    public DriverEquipmentExperience? EquipmentExperience { get; set; }

    public ICollection<DocumentFile> Documents { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverEmployment> EmploymentHistory { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverEducation> EducationHistory { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverCertification> Certifications { get; set; } = [];

    [ValidateComplexType] public ICollection<DriverViolation> ViolationHistory { get; set; } = [];

    public IEnumerable<TrailerType> AllTrailerTypes =>
        EmploymentHistory.SelectMany(e => e.TrailerTypes.Select(t => t.TrailerType)).Distinct();

    /// <summary>
    /// Returns a 0-100 completeness score based on required and optional hiring criteria.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CompletenessScore
    {
        get
        {
            int score = 0;
            
            // Required items (16 points each, max 80)
            if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(PhoneNumber))
                score += 16;
            
            if (License != null && License.ExpiryDate > DateOnly.FromDateTime(DateTime.Today))
                score += 16;
                
            if (MedicalCard != null && MedicalCard.ExpiryDate > DateOnly.FromDateTime(DateTime.Today))
                score += 16;
                
            if (EmploymentHistory.Any())
                score += 16;
                
            if (YearsOfExperience > 0 && TotalMilesDriven > 0)
                score += 16;

            // Optional items (5 points each, max 15)
            if (Skills.Any())
                score += 5;
                
            if (Certifications.Any())
                score += 5;
                
            if (EducationHistory.Any())
                score += 5;

            return score;
        }
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string CompletenessLabel => CompletenessScore switch
    {
        >= 95 => "Complete",
        >= 80 => "Good",     // All required fields met
        >= 50 => "Needs work",
        _ => "Incomplete"
    };

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string CompletenessBadgeClass => CompletenessScore switch
    {
        >= 95 => "bg-success",
        >= 80 => "bg-info text-dark",
        >= 50 => "bg-warning text-dark",
        _ => "bg-danger"
    };

    [NotMapped]
    public List<string> MissingFields
    {
        get
        {
            var missing = new List<string>();
            
            // Required Fields
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(PhoneNumber))
                missing.Add("Basic Contact Info");
            
            if (License == null || License.ExpiryDate <= DateOnly.FromDateTime(DateTime.Today))
                missing.Add("Valid CDL License");

            if (MedicalCard == null || MedicalCard.ExpiryDate <= DateOnly.FromDateTime(DateTime.Today))
                missing.Add("Valid DOT Medical Card");

            if (!EmploymentHistory.Any())
                missing.Add("Employment History");

            if (YearsOfExperience <= 0 || TotalMilesDriven <= 0)
                missing.Add("Driving Experience");

            // Optional Fields
            if (!Certifications.Any())
                missing.Add("Certifications (Optional)");
                
            if (!Skills.Any())
                missing.Add("Skills (Optional)");
                
            if (!EducationHistory.Any())
                missing.Add("Education (Optional)");

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