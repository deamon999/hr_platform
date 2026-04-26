using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class JobApplication
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public Job Job { get; set; } = default!;
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(1000)] public string? ReviewerNotes { get; set; }
}