using System;
using System.Globalization;

namespace EpicWorkflow.Helpers
{
    public static class DateTimeExtensions
    {
        private static CultureInfo CiRu = new CultureInfo("ru-RU");

        public static string ToDd(this DateTime dateTime)
        {
            return dateTime.ToString("dd", CiRu);
        }

        public static string ToDdMmm(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMM", CiRu);
        }

        public static string ToMmmm(this DateTime dateTime)
        {
            return dateTime.ToString("MMMM", CiRu);
        }

        public static string ToDdMmmYyyy(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMM yyyy", CiRu);
        }

        public static DateTime NextDayOfWeek(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            return dateTime.AddDays((7 - (int) dateTime.DayOfWeek + (int) dayOfWeek) % 7);
        }
    }
}