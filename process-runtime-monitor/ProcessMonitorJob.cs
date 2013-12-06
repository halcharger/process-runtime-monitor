using System;
using Quartz;

namespace process_runtime_monitor
{
    public class ProcessMonitorJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("running ProcessMonitorJob {0}", DateTime.Now.ToLongTimeString());
        }
    }
}