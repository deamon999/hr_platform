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

    // Navigation property for trailer types junction table
    public ICollection<DriverEmploymentTrailerType> TrailerTypes { get; set; } = [];

    public PayPeriod? PayType { get; set; }
    
    public bool MayWeContact { get; set; } = true;
    
    [MaxLength(20)] public string? CompanyPhone { get; set; }
    
    [MaxLength(100)] public string? CompanyEmail { get; set; }

    // Helper methods for managing trailer types
    public bool HasTrailerType(TrailerType trailerType) =>
        TrailerTypes.Any(t => t.TrailerType == trailerType);

    public void AddTrailerType(TrailerType trailerType)
    {
        if (!HasTrailerType(trailerType))
        {
            TrailerTypes.Add(new DriverEmploymentTrailerType { TrailerType = trailerType });
        }
    }

    public void RemoveTrailerType(TrailerType trailerType)
    {
        var existing = TrailerTypes.FirstOrDefault(t => t.TrailerType == trailerType);
        if (existing != null)
        {
            TrailerTypes.Remove(existing);
        }
    }

    public IEnumerable<TrailerType> GetTrailerTypeValues() =>
        TrailerTypes.Select(t => t.TrailerType);
}