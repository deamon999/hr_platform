namespace HrPlatform.Services;

public interface IUserTimeZoneService
{
    TimeZoneInfo TimeZone { get; }
    bool IsInitialized { get; }
    void SetTimeZone(string ianaTimeZoneId);
    DateTime? ToUserTime(DateTime? utcTime);
    DateTime ToUserTime(DateTime utcTime);
}