using System;
using Quartz;

namespace process_runtime_monitor
{
    public class ProcessMonitorJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            if (Program.ProcessesToMonitor.Count == 0)
                Console.WriteLine("No processes to monitor: {0}", DateTime.Now.ToLongTimeString());
            else
                Console.WriteLine("Monitoring for processes: {0} at {1}", string.Join(", ", Program.ProcessesToMonitor), DateTime.Now.ToLongTimeString());
        }
    }
}