using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverEducation
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required] public EducationLevel Level { get; set; }

    [MaxLength(200)] public string? InstitutionName { get; set; }

    [MaxLength(100)] public string? City { get; set; }

    [MaxLength(50)] public string? State { get; set; }

    [MaxLength(150)] public string? FieldOfStudy { get; set; }

    public DateOnly? GraduationDate { get; set; }

    public bool Graduated { get; set; }
}