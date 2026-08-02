using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverMedicalCard
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required] public DateOnly IssuedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required] public DateOnly ExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [MaxLength(150)] public string? MedicalExaminerName { get; set; }

    [MaxLength(50)] public string? MedicalExaminerCertNumber { get; set; }

    [Required] public SelfCertificationCategory? SelfCertification { get; set; }
}