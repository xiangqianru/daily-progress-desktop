using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace DailyProgressDesk
{
    public class AlmanacEntry
    {
        public string Lunar { get; set; }
        public string Yi { get; set; }
        public string Ji { get; set; }

        public AlmanacEntry()
        {
            Lunar = "";
            Yi = "";
            Ji = "";
        }
    }

    public static class AlmanacService
    {
        private const string ResourceName = "DailyProgressDesk.almanac.json";
        private static readonly object SyncRoot = new object();
        private static Dictionary<string, AlmanacEntry> entries;

        public static AlmanacEntry Get(DateTime date)
        {
            EnsureLoaded();
            string key = date.ToString("yyyy-MM-dd");
            AlmanacEntry entry;
            if (entries != null && entries.TryGetValue(key, out entry)) return entry;

            return new AlmanacEntry
            {
                Lunar = BuildLunarFallback(date),
                Yi = "数据范围外，暂不提供",
                Ji = "数据范围外，暂不提供"
            };
        }

        private static void EnsureLoaded()
        {
            if (entries != null) return;
            lock (SyncRoot)
            {
                if (entries != null) return;
                entries = new Dictionary<string, AlmanacEntry>();
                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using (Stream stream = assembly.GetManifestResourceStream(ResourceName))
                    {
                        if (stream == null) return;
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            JavaScriptSerializer serializer = new JavaScriptSerializer();
                            serializer.MaxJsonLength = 32 * 1024 * 1024;
                            Dictionary<string, AlmanacEntry> loaded =
                                serializer.Deserialize<Dictionary<string, AlmanacEntry>>(reader.ReadToEnd());
                            if (loaded != null) entries = loaded;
                        }
                    }
                }
                catch
                {
                    entries = new Dictionary<string, AlmanacEntry>();
                }
            }
        }

        private static string BuildLunarFallback(DateTime date)
        {
            try
            {
                ChineseLunisolarCalendar calendar = new ChineseLunisolarCalendar();
                int year = calendar.GetYear(date);
                int month = calendar.GetMonth(date);
                int day = calendar.GetDayOfMonth(date);
                int leapMonth = calendar.GetLeapMonth(year);
                bool isLeap = leapMonth > 0 && month == leapMonth;
                if (leapMonth > 0 && month >= leapMonth) month--;

                string[] months = { "", "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊" };
                return "农历 " + (isLeap ? "闰" : "") + months[month] + "月" + LunarDay(day);
            }
            catch
            {
                return "农历日期暂不可用";
            }
        }

        private static string LunarDay(int day)
        {
            string[] digits = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            if (day <= 10) return day == 10 ? "初十" : "初" + digits[day];
            if (day < 20) return "十" + digits[day - 10];
            if (day == 20) return "二十";
            if (day < 30) return "廿" + digits[day - 20];
            return "三十";
        }
    }
}
