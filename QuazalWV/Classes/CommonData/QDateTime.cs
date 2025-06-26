using System;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class QDateTime : IData
    {
        public ulong RawTime { get; set; }
        public DateTime Time { get; set; }
        const uint MonthMask = 0x3C00000;
        const uint DayMask = 0x3E0000;
        const uint HourMask = 0x1F000;
        const uint MinuteMask = 0xFC0;
        const uint SecondMask = 0x3F;

        public QDateTime(DateTime time)
        {
            Time = time;
            RawTime = ToU64(Time);
        }

        public QDateTime(ulong rawTime)
        {
            RawTime = rawTime;
            Time = ToDateTime(rawTime);
        }

        public QDateTime(Stream s)
        {
            FromStream(s);
            if (RawTime != 0 && ( (RawTime >> 56 & 0xff ) != 0xff ) )
                Time = ToDateTime(RawTime);
            else
            {
                Log.WriteLine(1, $"[QDateTime] Invalid time: {RawTime}", Color.Red);
                Time = DateTime.Now;
            }
        }

        public void FromStream(Stream s)
        {
            RawTime = Helper.ReadU64DateTime(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU64(s, RawTime);
        }

        private static DateTime ToDateTime(ulong raw)
        {
            var year = (int)(raw >> 26);
            var month = (int)((raw & MonthMask) >> 22);
            var day = (int)((raw & DayMask) >> 17);
            var hour = (int)((raw & HourMask) >> 12);
            var minute = (int)((raw & MinuteMask) >> 6);
            var second = (int)(raw & SecondMask);
            return new DateTime(year, month, day, hour, minute, second);
        }

        private ulong ToU64(DateTime time)
        {
            ulong result = 0;
            result += (ulong)time.Year << 26;
            result += (ulong)time.Month << 22;
            result += (ulong)time.Day << 17;
            result += (ulong)time.Hour << 12;
            result += (ulong)time.Minute << 6;
            result += (ulong)time.Second;
            return result;
        }
    }
}
