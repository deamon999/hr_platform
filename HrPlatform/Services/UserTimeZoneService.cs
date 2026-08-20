using TimeZoneConverter;

namespace HrPlatform.Services;

public class UserTimeZoneService : IUserTimeZoneService
{
    public TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.Utc;
    public bool IsInitialized { get; private set; }

    public void SetTimeZone(string ianaTimeZoneId)
    {
        try
        {
            TimeZone = TZConvert.GetTimeZoneInfo(ianaTimeZoneId);
            IsInitialized = true;
        }
        catch
        {
            // Fallback to UTC if time zone ID is not recognized
            TimeZone = TimeZoneInfo.Utc;
            IsInitialized = true;
        }
    }

    public DateTime? ToUserTime(DateTime? utcTime)
    {
        if (utcTime == null) return null;
        
        var time = utcTime.Value;
        if (time.Kind == DateTimeKind.Unspecified)
            time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
            
        return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZone);
    }
    
    public DateTime ToUserTime(DateTime utcTime)
    {
        if (utcTime.Kind == DateTimeKind.Unspecified)
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZone);
    }
}
