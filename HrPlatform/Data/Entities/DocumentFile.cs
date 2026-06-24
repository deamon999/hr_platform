using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Data.Entities;

public class DocumentFile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = default!;

    [Required]
    public byte[] Data { get; set; } = [];

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
