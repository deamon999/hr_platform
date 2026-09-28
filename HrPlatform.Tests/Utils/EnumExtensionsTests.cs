using HrPlatform.Data.Enums;
using HrPlatform.Utils;
using Xunit;

namespace HrPlatform.Tests.Utils;

public class EnumExtensionsTests
{
    [Fact]
    public void GetDescriptionValue_WithDescriptionAttribute_ReturnsDescription()
    {
        Assert.Equal("Hazardous Materials", CdlEndorsement.Hazmat.GetDescriptionValue());
        Assert.Equal("Tank Vehicles", CdlEndorsement.Tanker.GetDescriptionValue());
        Assert.Equal("Double/Triple Trailers", CdlEndorsement.Doubles.GetDescriptionValue());
        Assert.Equal("Needs to be Contacted", LeadStatus.NeedsToBeContacted.GetDescriptionValue());
        Assert.Equal("Ready in 2 Weeks", LeadStatus.ReadyIn2Weeks.GetDescriptionValue());
    }

    [Fact]
    public void GetDescriptionValue_WithoutDescriptionAttribute_ReturnsNull()
    {
        Assert.Null(ApplicationStatus.Pending.GetDescriptionValue());
        Assert.Null(ApplicationStatus.Accepted.GetDescriptionValue());
        Assert.Null(ApplicationStatus.Rejected.GetDescriptionValue());
    }

    [Fact]
    public void GetDescriptionValue_RepeatedCalls_ReturnsConsistentCachedResults()
    {
        var first = CdlEndorsement.Hazmat.GetDescriptionValue();
        var second = CdlEndorsement.Hazmat.GetDescriptionValue();
        Assert.Same(first, second);
    }
}
