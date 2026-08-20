using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum TrailerType
{
    [Description("Dry Van")] DryVan,
    [Description("Flatbed")] Flatbed,
    [Description("Reefer")] Reefer,
    [Description("Tanker")] Tanker,
    [Description("Step Deck")] StepDeck,
    [Description("Lowboy")] Lowboy,
    [Description("Doubles")] Doubles,
    [Description("Triples")] Triples,
    [Description("Car Hauler")] CarHauler,
    [Description("Intermodal")] Intermodal,
    [Description("RGN")] RGN,
    [Description("Conestoga")] Conestoga,
    [Description("Other")] Other
}