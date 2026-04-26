using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverEmployment
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required] [MaxLength(150)] public string JobTitle { get; set; } = default!;

    [Required] [MaxLength(150)] public string CompanyName { get; set; } = default!;

    [MaxLength(100)] public string? City { get; set; }

    [MaxLength(50)] public string? State { get; set; }

    [Required] public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [MaxLength(500)] public string? ReasonForLeaving { get; set; }

    public int? AverageWeeklyMiles { get; set; }

    // Stored as comma-delimited string via EF value conversion — no junction table needed
    public List<TrailerType> TrailerTypes { get; set; } = [];

    [MaxLength(2000)] public string? Responsibilities { get; set; }
}