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

    // Navigation property for endorsements junction table
    public ICollection<DriverLicenseEndorsement> Endorsements { get; set; } = [];

    [MaxLength(200)] public string? Restrictions { get; set; }

    [Required] public DateOnly IssuedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required] public DateOnly ExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [MaxLength(500)]
    public string? DocumentBlobPath { get; set; }

    public DateTime? DocumentUploadedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool HasDocument => !string.IsNullOrEmpty(DocumentBlobPath);

    // Helper methods for managing endorsements
    public bool HasEndorsement(CdlEndorsement endorsement) =>
        Endorsements.Any(e => e.Endorsement == endorsement);

    public void AddEndorsement(CdlEndorsement endorsement)
    {
        if (!HasEndorsement(endorsement))
        {
            Endorsements.Add(new DriverLicenseEndorsement { Endorsement = endorsement });
        }
    }

    public void RemoveEndorsement(CdlEndorsement endorsement)
    {
        var existing = Endorsements.FirstOrDefault(e => e.Endorsement == endorsement);
        if (existing != null)
        {
            Endorsements.Remove(existing);
        }
    }

    public IEnumerable<CdlEndorsement> GetEndorsementValues() =>
        Endorsements.Select(e => e.Endorsement);
}