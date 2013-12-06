using Quartz;
using Topshelf;
using Topshelf.Quartz;

namespace process_runtime_monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(c => c.ScheduleQuartzJobAsService(q => q.WithJob(() => JobBuilder.Create<ProcessMonitorJob>().Build())
                                                                    .AddTrigger(() => TriggerBuilder.Create()
                                                                                                    .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(10).RepeatForever())
                                                                                                    .Build())));
        }
    }
}
