using System;

namespace EduMatch.BusinessLogicLayer.Utils
{
    public static class VietnamTimeProvider
    {
        private static readonly string[] TimeZoneIds = new[]
        {
            "SE Asia Standard Time",       // Windows
            "Asia/Ho_Chi_Minh"             // Linux/macOS
        };

        /// <summary>
        /// Returns current time in Vietnam time zone (UTC+7 fallback).
        /// </summary>
        public static DateTime Now()
        {
            foreach (var tzId in TimeZoneIds)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            // Fallback if timezone not found
            return DateTime.UtcNow.AddHours(7);
        }
    }
}
