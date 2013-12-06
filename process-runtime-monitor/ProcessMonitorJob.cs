using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quartz;

namespace process_runtime_monitor
{
    public class ProcessMonitorJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Running job {0}", DateTime.Now.ToLongTimeString());

            if (Program.ProcessesToMonitor.Count == 0) return;

            var runningProcesses = Process.GetProcesses().Where(p => Program.ProcessesToMonitor.Contains(p.ProcessName)).Select(p => new RunningProcess { PID = p.Id.ToString(), Name = p.ProcessName });
            UpdateStoppedProcesses(runningProcesses);
            UpdateStartedProcesses(runningProcesses);

            foreach (var runningProcess in runningProcesses)
                Console.WriteLine("Found running process: {0} [{1}] at {2}", runningProcess.Name, runningProcess.PID, DateTime.Now.ToLongTimeString());
        }

        private void UpdateStartedProcesses(IEnumerable<RunningProcess> runningProcesses)
        {
            var newlystartedProcesses = from rp in runningProcesses
                                        join p in Program.Processes
                                        on rp equals p into boom
                                        from p in boom.DefaultIfEmpty()
                                        where p == null
                                        select new RunningProcess{PID = rp.PID, Name = rp.Name, Started = DateTime.Now};

            Program.Processes.AddRange(newlystartedProcesses);
        }

        private void UpdateStoppedProcesses(IEnumerable<RunningProcess> runningProcesses)
        {
            
        }
    }
}