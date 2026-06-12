using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverEmploymentTrailerType
{
    public int Id { get; set; }
    public int DriverEmploymentId { get; set; }
    public DriverEmployment DriverEmployment { get; set; } = default!;
    public TrailerType TrailerType { get; set; }
}

