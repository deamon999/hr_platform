using HrPlatform.Services;

namespace HrPlatform.Tests.Services;

public class UserTimeZoneServiceTests
{
    [Fact]
    public void SetTimeZone_ValidIanaZone_SetsTimeZoneAndIsInitialized()
    {
        // Arrange
        var service = new UserTimeZoneService();

        // Act
        service.SetTimeZone("America/Chicago");

        // Assert
        Assert.True(service.IsInitialized);
        Assert.Equal("Central Standard Time", service.TimeZone.Id);
    }

    [Fact]
    public void SetTimeZone_InvalidZone_FallsBackToUtc()
    {
        // Arrange
        var service = new UserTimeZoneService();

        // Act
        service.SetTimeZone("Invalid/Zone");

        // Assert
        Assert.True(service.IsInitialized);
        Assert.Equal(TimeZoneInfo.Utc.Id, service.TimeZone.Id);
    }

    [Fact]
    public void ToUserTime_NonNullable_ConvertsCorrectly()
    {
        // Arrange
        var service = new UserTimeZoneService();
        service.SetTimeZone("America/Chicago");
        var utcTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 PM UTC

        // Act
        var localTime = service.ToUserTime(utcTime);

        // Assert
        // Central Standard Time is UTC-6 in January
        Assert.Equal(new DateTime(2023, 1, 1, 6, 0, 0), localTime);
    }

    [Fact]
    public void ToUserTime_Nullable_ConvertsCorrectly()
    {
        // Arrange
        var service = new UserTimeZoneService();
        service.SetTimeZone("America/New_York");
        DateTime? utcTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 PM UTC

        // Act
        var localTime = service.ToUserTime(utcTime);

        // Assert
        // Eastern Standard Time is UTC-5 in January
        Assert.NotNull(localTime);
        Assert.Equal(new DateTime(2023, 1, 1, 7, 0, 0), localTime.Value);
    }

    [Fact]
    public void ToUserTime_Null_ReturnsNull()
    {
        // Arrange
        var service = new UserTimeZoneService();

        // Act
        var localTime = service.ToUserTime(null);

        // Assert
        Assert.Null(localTime);
    }
}