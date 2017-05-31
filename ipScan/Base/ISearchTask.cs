using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ipScan.Base
{
    interface ISearchTask<T, TSub>
    {
        object                              Locker { get; }

        bool                                isRunning { get; }        
        bool                                wasStopped { get; }
        int                                 taskId { get; }
        int                                 WorkingTaskCount { get; }
        Dictionary<object, Task>            Tasks { get; }
        int                                 MaxTaskCountLimit { get; set; }

        BufferedResult<T>                   Buffer { get; }        
        Dictionary<TSub, bool>              SubTaskStates { get; }
        Dictionary<uint, uint>              Progress { get; }
        uint                                FirstIPAddress { get; }
        uint                                CurrentPosition { get; }
        uint                                Count { get; set; }
        uint                                Remaind { get; }
                
        uint                                progress { get; }
        bool                                isPaused { get; }

        void                                Init(uint firstIPAddress, uint count);

        void                                Start();
        void                                Stop();
        void                                Pause(bool IsPaused);        
    }
}
