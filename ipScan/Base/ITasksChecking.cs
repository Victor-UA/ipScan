using ipScan.Base;
using System;
using System.Collections.Generic;

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
        void Pause(bool IsPaused);

        bool ChangeSearchTaskRange(int Index);

        bool IsStarting { get; set; }

        int TasksCount { get; }

    }
}
