namespace SlurkExp.Data
{
    public static class UserTimeZone
    {
        public static string _userLocalTimeZone = "Europe/Zurich";

        public static string GetUIDateString(this DateTime datetime)
        {
            return GetUIString(datetime, "dd.MM.yyyy");
        }
        public static string GetUITimeString(this DateTime datetime)
        {
            return GetUIString(datetime, "HH:mm");
        }
        public static string GetUIDateTimeString(this DateTime datetime)
        {
            return GetUIString(datetime, "dd.MM.yyyy HH:mm:ss");
        }
        public static string GetUIString(this DateTime datetime, string format)
        {
            if (datetime == DateTime.MinValue) return String.Empty; //TODO: add overloaded function with arg to render min value?

            return datetime.UtcToLocalUserTime().ToString(format);
        }

        public static DateTime UtcToLocalUserTime(this DateTime datetime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(datetime, TimeZoneInfo.FindSystemTimeZoneById(_userLocalTimeZone));
        }

        public static DateTime LocalUserTimeToUtc(this DateTime datetime, bool ignoreMinDate = true)
        {
            if (ignoreMinDate && datetime == DateTime.MinValue) return datetime; // by default, do not convert minvalue
            return TimeZoneInfo.ConvertTimeToUtc(datetime, TimeZoneInfo.FindSystemTimeZoneById(_userLocalTimeZone));
        }
    }
}
