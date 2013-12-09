using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quartz;
using Topshelf.Logging;

namespace process_runtime_monitor
{
    public class ProcessMonitorJob : IJob
    {
        private readonly ProcessStorage storage = new ProcessStorage();
        static readonly LogWriter logger = HostLogger.Get<ProcessMonitorJob>();

        public void Execute(IJobExecutionContext context)
        {
            logger.DebugFormat("Running job {0}", DateTime.Now.ToLongTimeString());

            if (Program.ProcessesToMonitor.Count == 0) return;
            
            var runningProcesses = Process.GetProcesses().Where(p => Program.ProcessesToMonitor.Contains(p.ProcessName)).Select(p => new RunningProcess { PID = p.Id.ToString(), Name = p.ProcessName });
            UpdateStoppedProcesses(runningProcesses);
            UpdateStartedProcesses(runningProcesses);

            PrintOutCurrentlyMonitoredProcesses();
        }

        private void PrintOutCurrentlyMonitoredProcesses()
        {
            if (Program.Processes.Any())
            {
                logger.Info("Currently montitored processes:");
                Program.Processes.ForEach(p => logger.InfoFormat("monitoring: {0}, started: {1}", p.Name, p.Started.ToLongTimeString()));
            }
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

            logger.InfoFormat("Found newly started processes: {0}", string.Join(", ", newlystartedProcesses.Select(p => p.Name)));
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

            logger.InfoFormat("Found newly stopped processes: {0}", string.Join(", ", newlyStoppedProcesses.Select(p => p.Name)));
            newlyStoppedProcesses.ToList().ForEach(SaveProcessAndRemoveFromStaticList);
        }

        private void SaveProcessAndRemoveFromStaticList(RunningProcess p)
        {
            p.Stopped = DateTime.Now;
            Program.Processes.Remove(p);
            storage.SaveOrUpdateProcess(p);
        }
    }
}