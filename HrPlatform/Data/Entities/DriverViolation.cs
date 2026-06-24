using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Data.Entities;

public class DriverViolation
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required]
    public ViolationType Type { get; set; }

    [Required]
    public DateOnly OccurredDate { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool AtFault { get; set; }

    // Reportable = must be disclosed to DOT
    public bool Reportable { get; set; } = true;

    // Resolution: dismissed, fine paid, license suspension, etc.
    [MaxLength(200)]
    public string? Resolution { get; set; }
}
