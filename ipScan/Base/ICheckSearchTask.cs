using System;

namespace ipScan.Base
{
    interface ICheckSearchTask
    {
        DateTime lastTime { get; }

        void Check();
        void Stop();
        void Pause();
        void Pause(bool IsPaused);
        
    }
}
