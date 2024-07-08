using System;
using TimeZoneConverter;

namespace DiningHub.Helper
{
    public static class DateTimeHelper
    {
        public static DateTime GetMalaysiaTime()
        {
            var malaysiaTimeZone = TZConvert.GetTimeZoneInfo("Asia/Kuala_Lumpur");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, malaysiaTimeZone);
        }
    }
}
