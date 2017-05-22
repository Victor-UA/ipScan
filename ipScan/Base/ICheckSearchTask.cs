using System;

namespace ipScan.Base
{
    interface ICheckSearchTask
    {
        DateTime LastTime { get; }

        void Check();
        void Stop();
        bool Pause();
        bool Pause(bool IsPaused);
        
    }
}
