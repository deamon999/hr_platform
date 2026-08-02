using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrPlatform.Data.Models;

public class DriverEquipmentExperience
{
    public int Id { get; set; }
    
    public int DriverProfileId { get; set; }
    public DriverProfile? DriverProfile { get; set; }

    // Equipment Experience (Years)
    public int DryVan { get; set; }
    public int Reefer { get; set; }
    public int Flatbed { get; set; }
    public int StepDeck { get; set; }
    public int Rgn { get; set; }
    public int Lowboy { get; set; }
    public int Tanker { get; set; }
    public int CarHauler { get; set; }
    public int Pneumatic { get; set; }
    public int Dump { get; set; }

    // Other Experience Flags
    public bool AutomaticTransmission { get; set; }
    public bool CanadaExperience { get; set; }
    public bool HazmatEndorsement { get; set; }
    public bool MountainDriving { get; set; }
    public bool WinterDriving { get; set; }
    public bool NycExperience { get; set; }
}
