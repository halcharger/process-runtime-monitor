using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace process_runtime_monitor
{
    public class ProcessTableEntity : TableEntity
    {
        private const string timeFormat = "HH:mm";

        public string TimesRun { get; set; }
        public double TotalMinutesRun { get; set; }

        public void SetTotalMinutesRun()
        {
            TotalMinutesRun = 0;

            if (string.IsNullOrEmpty(TimesRun)) return;

            TotalMinutesRun = TimesRun.Split(';').Sum(ts => GetTimeSpanFromTimeSlot(ts).TotalMinutes);
        }

        public void AddRunTimes(DateTime started, DateTime stopped)
        {
            var from = started.ToString(timeFormat);
            var to = stopped.ToString(timeFormat);
            var fromTo = from + "|" + to;

            TimesRun += string.IsNullOrEmpty(TimesRun) ? fromTo : ";" + fromTo;
            SetTotalMinutesRun();
        }

        private TimeSpan GetTimeSpanFromTimeSlot(string timeSlot)
        {
            var times = timeSlot.Split('|');
            var from = DateTime.ParseExact(times[0], timeFormat, System.Globalization.CultureInfo.CurrentCulture);
            var to = DateTime.ParseExact(times[1], timeFormat, System.Globalization.CultureInfo.CurrentCulture);
            return to.Subtract(from);
        }
    }
}