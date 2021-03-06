﻿using System;

namespace process_runtime_monitor
{
    public class RunningProcess
    {
        public string PID { get; set; }
        public string Name { get; set; }
        public DateTime Started { get; set; }
        public DateTime Stopped { get; set; }
    }
}