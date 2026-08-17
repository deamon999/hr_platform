using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Xunit;
using System.Collections.Generic;

namespace HrPlatform.Tests.Services;

public class JobMatchServiceTests
{
    private readonly JobMatchService _service;

    public JobMatchServiceTests()
    {
        _service = new JobMatchService();
    }

    [Fact]
    public void ComputeScore_FullMatch_Returns100()
    {
        var job = new Job
        {
            Title = "T",
            RequiredCdlClass = CdlClass.A,
            RequiredEndorsements = new List<CdlEndorsement> { CdlEndorsement.Hazmat, CdlEndorsement.Tanker },
            MinYearsExperience = 5,
            RequiredTrailerType = TrailerType.DryVan,
            RouteType = RouteType.Local,
            RequiresManualTransmission = true,
            IsTeamDriving = false,
            AllowsPets = true,
            AllowsRiders = true
        };

        var profile = new DriverProfile
        {
            UserId="1", FirstName="F", LastName="L", PhoneNumber="1", Email="e@e.com",
            License = new DriverLicense
            {
                Class = CdlClass.A, IssuingState="TX", LicenseNumber="1",
                Endorsements = new List<DriverLicenseEndorsement> 
                { 
                    new() { Endorsement = CdlEndorsement.Hazmat }, 
                    new() { Endorsement = CdlEndorsement.Tanker } 
                }
            },
            YearsOfExperience = 5,
            EmploymentHistory = new List<DriverEmployment>
            {
                new()
                {
                    TrailerTypes = new List<DriverEmploymentTrailerType>
                    {
                        new() { TrailerType = TrailerType.DryVan }
                    }
                }
            },
            PreferredRouteType = RouteType.Local,
            CanDriveManual = true,
            CanDriveInTeam = false,
            WantsToDriveWithPets = true,
            WantsToDriveWithRiders = true
        };

        var score = _service.ComputeScore(profile, job);

        Assert.Equal(100, score);
    }

    [Fact]
    public void ComputeScore_NoRequirements_Returns100()
    {
        var job = new Job { Title = "T" };
        var profile = new DriverProfile { UserId="1", FirstName="F", LastName="L", PhoneNumber="1", Email="e@e.com" };

        var score = _service.ComputeScore(profile, job);

        Assert.Equal(100, score);
    }

    [Fact]
    public void ComputeScore_PartialMatch_CalculatesCorrectly()
    {
        var job = new Job
        {
            Title = "T",
            RequiredCdlClass = CdlClass.A, 
            MinYearsExperience = 10, 
            RequiredTrailerType = TrailerType.Flatbed 
        };

        var profile = new DriverProfile
        {
            UserId="1", FirstName="F", LastName="L", PhoneNumber="1", Email="e@e.com",
            License = new DriverLicense { Class = CdlClass.B, IssuingState="TX", LicenseNumber="1" }, 
            YearsOfExperience = 5, 
            EmploymentHistory = new List<DriverEmployment>
            {
                new()
                {
                    TrailerTypes = new List<DriverEmploymentTrailerType>
                    {
                        new() { TrailerType = TrailerType.Flatbed }
                    }
                }
            },
            CanDriveManual = true, 
            CanDriveInTeam = false, 
            WantsToDriveWithPets = false, 
            WantsToDriveWithRiders = false 
        };

        var score = _service.ComputeScore(profile, job);

        Assert.Equal(61, score);
    }
}
