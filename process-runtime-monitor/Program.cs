using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Quartz;
using Topshelf;
using Topshelf.Quartz;

namespace process_runtime_monitor
{
    public class Program
    {
        public static List<RunningProcess> Processes = new List<RunningProcess>();
        public static List<string> ProcessesToMonitor = new List<string>();

        static void Main(string[] args)
        {
            ConfigureProcessesToMonitor();
            ConfigureProcessMonitorJob();
        }

        static void ConfigureProcessesToMonitor()
        {
            ProcessesToMonitor = (ConfigurationManager.AppSettings["processes-to-monitor"] ?? string.Empty).Split(',').ToList();
        }

        static void ConfigureProcessMonitorJob()
        {
            HostFactory.Run(c =>
            {
                c.UseLog4Net();
                log4net.Config.XmlConfigurator.Configure();

                c.ScheduleQuartzJobAsService(q => q.WithJob(() => JobBuilder.Create<ProcessMonitorJob>().Build())
                    .AddTrigger(() => TriggerBuilder.Create()
                        .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(5).RepeatForever())
                        .Build()));

            });
        }
    }
}
