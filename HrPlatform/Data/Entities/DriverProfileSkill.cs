using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Data.Models;

public class DriverProfileSkill
{
    public int Id { get; set; }
    public int DriverProfileId { get; set; }
    public DriverProfile DriverProfile { get; set; } = default!;
    
    [Required] [MaxLength(100)] public string Skill { get; set; } = default!;
}

