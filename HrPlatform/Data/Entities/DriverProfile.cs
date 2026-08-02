using System.ComponentModel.DataAnnotations;
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
    
    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.ActivelyLooking;
    
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
    public RouteType? PreferredRouteType { get; set; }
    public string? PreferredPosition { get; set; }
    public List<string>? PreferredFreight { get; set; } = new();
    public List<string>? PreferredRegions { get; set; } = new();
    public string? MinimumWeeklyPay { get; set; }
    public DateOnly? AvailableStartDate { get; set; }
    
    public bool CanDriveManual { get; set; }
    public bool WantsToDriveWithPets { get; set; }
    public bool WantsToDriveWithRiders { get; set; }
    public bool CanDriveInTeam { get; set; }

    public int YearsOfExperience { get; set; }
    public int OtrExperience { get; set; }
    public int LocalExperience { get; set; }
    public int OwnerOperatorExperience { get; set; }

    // Equipment Experience (Years)
    public int DryVanExperience { get; set; }
    public int ReeferExperience { get; set; }
    public int FlatbedExperience { get; set; }
    public int StepDeckExperience { get; set; }
    public int RgnExperience { get; set; }
    public int LowboyExperience { get; set; }
    public int TankerExperience { get; set; }
    public int CarHaulerExperience { get; set; }
    public int PneumaticExperience { get; set; }
    public int DumpExperience { get; set; }

    // Safety & Background
    public bool HasLicenseSuspension { get; set; }
    public DateOnly? LicenseSuspensionDate { get; set; }
    public string? LicenseSuspensionReason { get; set; }
    public bool HasFailedDrugTest { get; set; }
    public bool HasRefusedDrugTest { get; set; }
    public bool HasCompletedSAPProgram { get; set; }

    public ICollection<DriverEducation> Educations { get; set; } = [];

    // Military Service
    public bool HasMilitaryService { get; set; }
    public string? MilitaryBranch { get; set; }
    public int? MilitaryYears { get; set; }

    // Consents & Authorizations
    public bool ConsentPSP { get; set; }
    public bool ConsentMVR { get; set; }
    public bool ConsentClearinghouse { get; set; }
    public bool ConsentEmployment { get; set; }
    public string? ElectronicSignatureName { get; set; }
    public DateOnly? SignatureDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DriverLicense? License { get; set; }

    public DriverMedicalCard? MedicalCard { get; set; }

    // Navigation property for skills junction table
    public ICollection<DriverProfileSkill> Skills { get; set; } = [];

    public ICollection<DocumentFile> Documents { get; set; } = [];

    public ICollection<DriverEmployment> EmploymentHistory { get; set; } = [];

    public ICollection<DriverViolation> ViolationHistory { get; set; } = [];

    public IEnumerable<TrailerType> AllTrailerTypes =>
        EmploymentHistory.SelectMany(e => e.TrailerTypes.Select(t => t.TrailerType)).Distinct();

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