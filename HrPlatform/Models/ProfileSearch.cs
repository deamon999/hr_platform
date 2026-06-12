using HrPlatform.Data.Enums;

namespace HrPlatform.Models;

public record ProfileSearch
{
    public string? Name { get; set; }
    public CdlClass? CdlClass { get; set; }
    public CdlEndorsement? RequiredEndorsement { get; set; }
    public int? MinYears { get; set; }
    public AvailabilityStatus? Availability { get; set; }
}