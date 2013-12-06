using System.Collections.Generic;

namespace process_runtime_monitor
{
    public class ProcessStore
    {
        public static List<Process> Processes = new List<Process>();
        public static List<string> ProcessesToMonitor = new List<string>();
    }
}