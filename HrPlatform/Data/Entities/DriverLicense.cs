using System.ComponentModel.DataAnnotations;
using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverLicense
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required] [MaxLength(50)] public string LicenseNumber { get; set; } = default!;

    [Required] [MaxLength(2)] public string IssuingState { get; set; } = default!;

    [Required] public CdlClass Class { get; set; }

    // Stored as "Hazmat,Tanker" — no junction table needed
    public List<CdlEndorsement> Endorsements { get; set; } = [];

    [MaxLength(200)] public string? Restrictions { get; set; }

    [Required] public DateOnly IssuedDate { get; set; }

    [Required] public DateOnly ExpiryDate { get; set; }
}