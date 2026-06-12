using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum CdlEndorsement
{
    [Description("Hazardous Materials")] Hazmat,
    [Description("Tank Vehicles")] Tanker,

    [Description("Double/Triple Trailers")]
    Doubles,
    [Description("Passenger Transport")] Passenger,
    [Description("School Bus")] SchoolBus,

    [Description("Hazardous Materials + Tank Vehicles Combination")]
    HazTanker
}