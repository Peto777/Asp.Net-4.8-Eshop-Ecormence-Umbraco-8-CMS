using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace eshoppgsoftweb.lib.Util
{
    public class DateTimeUtil
    {
        public static string GetDateId(DateTime dt)
        {
            return dt.ToString("yyyyMMdd");
        }
        public static DateTime? DateIdToDate(string dateId)
        {
            try
            {
                int day = int.Parse(dateId.Substring(6, 2));
                int month = int.Parse(dateId.Substring(4, 2));
                int year = int.Parse(dateId.Substring(0, 4));

                return new DateTime(year, month, day); ;
            }
            catch
            {
                return null;
            }
        }

        public static string GetDisplayDate(DateTime dt)
        {
            return dt.ToString("dd.MM.yyyy");
        }
        public static string GetDisplayDate(DateTime? dt)
        {
            return dt != null ? GetDisplayDate(dt.Value) : string.Empty;
        }

        public static DateTime? DisplayDateToDate(string displayDate, string separator = null)
        {
            try
            {
                string itemSeparator = string.IsNullOrEmpty(separator) ? "." : separator;
                string[] items = Regex.Split(displayDate.Replace(itemSeparator, ";").Replace(" ", ";"), ";");
                int day = int.Parse(items[0]);
                int month = int.Parse(items[1]);
                int year = int.Parse(items[2]);

                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        public static string GetDisplayDateTime(DateTime dt)
        {
            return dt.ToString("dd.MM.yyyy HH:mm");
        }
        public static string GetDisplayDateTime(DateTime? dt)
        {
            return dt != null ? GetDisplayDateTime(dt.Value) : string.Empty;
        }

        public static DateTime DisplayDataToDateTime(string displayDateTime, DateTime defaultDateTime, string separator = null)
        {
            string displayDateTime_Sk = DisplaDateTimeToSkFormat(displayDateTime);
            try
            {
                string[] items = Regex.Split(displayDateTime_Sk.Trim().Replace(" ", ";"), ";");

                return DisplayDataToDate(items[0], items[1], defaultDateTime, separator);
            }
            catch
            {
                return defaultDateTime;
            }
        }

        public static string DisplaDateTimeToSkFormat(string displayDateTime)
        {
            try
            {
                if (!displayDateTime.Contains("/"))
                {
                    // Not EN format
                    return displayDateTime;
                }

                // MM/DD/YYYY HH:mm A
                string[] items = displayDateTime.Split(' ');

                // date
                string[] dateItems = items[0].Split('/');
                string month = dateItems[0];
                string day = dateItems[1];
                string year = dateItems[2];

                // time
                string[] timeItems = items[1].Split(':');
                string strHours = timeItems[0].TrimStart('0');
                int hours = string.IsNullOrEmpty(strHours) ? 0 : int.Parse(timeItems[0].TrimStart('0'));
                string strMinutess = timeItems[1].TrimStart('0');
                int minutes = string.IsNullOrEmpty(strMinutess) ? 0 : int.Parse(timeItems[1].TrimStart('0'));
                if (items.Length > 2 && items[2].ToLower() == "pm")
                {
                    if (hours < 12)
                    {
                        hours += 12;
                    }
                }

                return string.Format("{0}.{1}.{2} {3}:{4}",
                    day.PadLeft(2, '0'), month.PadLeft(2, '0'), year, hours, minutes.ToString().PadLeft(2, '0'));
            }
            catch
            {
                return displayDateTime;
            }
        }

        public static DateTime DisplayDataToDate(string displayDate, string displayTime, DateTime defaultDate, string separator = null)
        {
            DateTime? date = DisplayDateToDate(displayDate, separator);
            TimeSpan? time = DisplayTimeToTime(displayTime);

            if (date == null)
            {
                date = defaultDate.Date;
            }
            if (time == null)
            {
                time = defaultDate.TimeOfDay;
            }

            return CreateNewDateTime((DateTime)date, (TimeSpan)time);
        }
        private static DateTime CreateNewDateTime(DateTime date, TimeSpan time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
        }

        public static string GetDisplayTime(DateTime dt)
        {
            return dt.ToString("HH:mm");
        }
        public static string GetDisplayTime(DateTime? dt)
        {
            return dt != null ? dt.Value.ToString("HH:mm") : string.Empty;
        }

        public static TimeSpan? DisplayTimeToTime(string displayDate)
        {
            try
            {
                string[] items = Regex.Split(displayDate.Replace(":", ";"), ";");
                int hours = int.Parse(items[0]);
                int minutes = int.Parse(items[1]);

                return new TimeSpan(hours, minutes, 0);
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? DisplayTimeToDateTime(string displayTime)
        {
            if (string.IsNullOrEmpty(displayTime))
            {
                return null;
            }
            try
            {
                string[] items = Regex.Split(displayTime.Replace(":", ";"), ";");
                int hours = int.Parse(items[0]);
                int minutes = items.Length > 1 ? int.Parse(items[1]) : 0;

                DateTime today = DateTime.Today;
                return new DateTime(today.Year, today.Month, today.Day, hours, minutes, 0);
            }
            catch
            {
                return null;
            }
        }

        public static string GetMonthName(DateTime dt)
        {
            switch (dt.Month)
            {
                case 1:
                    return "január";
                case 2:
                    return "február";
                case 3:
                    return "marec";
                case 4:
                    return "apríl";
                case 5:
                    return "máj";
                case 6:
                    return "jún";
                case 7:
                    return "júl";
                case 8:
                    return "august";
                case 9:
                    return "september";
                case 10:
                    return "október";
                case 11:
                    return "november";
                case 12:
                    return "december";
            }

            return string.Empty;
        }

        public static string GetDayOfWeek(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Pondelok";
                case DayOfWeek.Tuesday:
                    return "Utorok";
                case DayOfWeek.Wednesday:
                    return "Streda";
                case DayOfWeek.Thursday:
                    return "Štvrtok";
                case DayOfWeek.Friday:
                    return "Piatok";
                case DayOfWeek.Saturday:
                    return "Sobota";
                case DayOfWeek.Sunday:
                    return "Nedeľa";
            }

            return string.Empty;
        }

        public static DateTime GetWeekMonday(DateTime dt)
        {
            while (dt.DayOfWeek != DayOfWeek.Monday)
            {
                dt = dt.AddDays(-1);
            }

            return dt;
        }

        public static int GetDayDiff(DateTime firstDate, DateTime secondDate)
        {
            DateTime dtFrom = firstDate < secondDate ? firstDate : secondDate;
            DateTime dtTo = firstDate < secondDate ? secondDate : firstDate;
            int idx = 0;
            while (dtFrom < dtTo)
            {
                dtFrom = dtFrom.AddDays(1);
                idx++;
            }

            return idx;
        }
    }

    public class DateAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || DateTimeUtil.DisplayDateToDate(value.ToString()) != null;
        }
    }
}
