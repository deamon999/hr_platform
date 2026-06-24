using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Data.Models;

public class DriverMedicalCard
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;

    [Required] public DateOnly IssuedDate { get; set; }

    [Required] public DateOnly ExpiryDate { get; set; }

    [MaxLength(150)] public string? MedicalExaminerName { get; set; }

    [MaxLength(50)] public string? MedicalExaminerCertNumber { get; set; }

    public bool SelfCertified { get; set; }

    [MaxLength(500)]
    public string? DocumentBlobPath { get; set; }

    public DateTime? DocumentUploadedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool HasDocument => !string.IsNullOrEmpty(DocumentBlobPath);
}