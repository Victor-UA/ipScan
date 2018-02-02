﻿using System;

namespace ipScan.Base
{
    interface ITasksChecking
    {
        DateTime LastTime { get; }
        TimeSpan loopTime { get; }

        int SleepTime { get; set; }
        bool MySearchTasksStartedAll { get;}

        void Check();
        void Stop();
        bool Pause();
        bool Pause(bool IsPaused);
        
    }
}