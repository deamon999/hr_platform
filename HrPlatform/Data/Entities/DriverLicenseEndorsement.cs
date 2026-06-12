using HrPlatform.Data.Enums;

namespace HrPlatform.Data.Models;

public class DriverLicenseEndorsement
{
    public int Id { get; set; }
    public int DriverLicenseId { get; set; }
    public DriverLicense DriverLicense { get; set; } = default!;
    public CdlEndorsement Endorsement { get; set; }
}

