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

            //foreach (var runningProcess in runningProcesses)
            //    Console.WriteLine("Found running process: {0} [{1}] at {2}", runningProcess.Name, runningProcess.PID, DateTime.Now.ToLongTimeString());
        }

        private void UpdateStartedProcesses(IEnumerable<RunningProcess> runningProcesses)
        {
            //get processes in runningProcesses that are not in Program.Processes using essentially a linq left join.
            var newlystartedProcesses = from rp in runningProcesses
                                        join p in Program.Processes
                                        on new{a=rp.PID, b=rp.Name} equals new {a=p.PID, b=p.Name} into boom
                                        from p in boom.DefaultIfEmpty()
                                        where p == null
                                        select new RunningProcess{PID = rp.PID, Name = rp.Name, Started = DateTime.Now};

            if (!newlystartedProcesses.Any()) return;

            Console.WriteLine("Found newly started processes: {0}", string.Join(", ", newlystartedProcesses.Select(p => p.Name)));
            Program.Processes.AddRange(newlystartedProcesses);
        }

        private void UpdateStoppedProcesses(IEnumerable<RunningProcess> runningProcesses)
        {
            //get processes in Program.Processes that are not in runningProcesses using essentially a linq left join.
            var newlyStoppedProcesses = from p in Program.Processes
                                        join rp in runningProcesses
                                        on new { a = p.PID, b = p.Name } equals new { a = rp.PID, b = rp.Name } into boom
                                        from rp in boom.DefaultIfEmpty()
                                        where rp == null
                                        select p;

            if (!newlyStoppedProcesses.Any()) return;

            Console.WriteLine("Found newly stopped processes: {0}", string.Join(", ", newlyStoppedProcesses.Select(p => p.Name)));
            newlyStoppedProcesses.ToList().ForEach(p => Program.Processes.Remove(p));
        }
    }
}