using System.Collections.Concurrent;
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
        Dictionary<object, Task>            Tasks { get; }

        BufferedResult<T>                   Buffer { get; }        
        ConcurrentDictionary<TSub, bool>    SubTaskStates { get; }
        Dictionary<uint, uint>              Progress { get; }
        uint                                FirstIPAddress { get; }
        uint                                CurrentPosition { get; }
        uint                                Count { get; set; }
        uint                                Remaind { get; }
                
        int                                 progress { get; }
        bool                                isPaused { get; }

        void                                Init(uint firstIPAddress, uint count);

        void                                Start();
        void                                Stop();
        void                                Pause(bool IsPaused);        
    }
}
