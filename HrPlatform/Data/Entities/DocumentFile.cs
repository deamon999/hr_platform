using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HrPlatform.Data.Models;

namespace HrPlatform.Data.Entities;

public class DocumentFile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required] [MaxLength(255)] public string FileName { get; set; } = default!;

    [Required] [MaxLength(100)] public string ContentType { get; set; } = default!;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int? DriverProfileId { get; set; }
    public DriverProfile? DriverProfile { get; set; }

    [MaxLength(50)] public string? DocumentType { get; set; }

    [MaxLength(2000)] public string? FilePath { get; set; }

    [NotMapped] public bool IsUnsaved { get; set; } = false;
}