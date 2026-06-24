using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Data.Models;

public class DriverCertification
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required] [MaxLength(150)] public string Name { get; set; } = default!;

    [MaxLength(100)] public string? IssuingAuthority { get; set; }

    [MaxLength(100)] public string? CertificationNumber { get; set; }

    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    [MaxLength(500)]
    public string? DocumentBlobPath { get; set; }

    public DateTime? DocumentUploadedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool HasDocument => !string.IsNullOrEmpty(DocumentBlobPath);
}