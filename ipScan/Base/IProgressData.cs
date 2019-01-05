using System;

namespace ipScan.Base
{
    interface IProgressData
    {
        int PauseTime { get; set; }
        int Progress { get; set; }
        int SubTasksCount { get; set; }
        int TasksCount { get; set; }
        TimeSpan TimeLeft { get; set; }
        TimeSpan TimePassed { get; set; }        
    }
}