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
        Dictionary<int, int>                Progress { get; }
        int                                 index { get; }
        int                                 currentPosition { get; }
        int                                 count { get; set; }
        int                                 remaind { get; }
                
        int                                 progress { get; }
        bool                                isPaused { get; }

        void                                Init(int Index, int Count);

        void                                Start();
        void                                Stop();
        void                                Pause(bool IsPaused);        
    }
}
