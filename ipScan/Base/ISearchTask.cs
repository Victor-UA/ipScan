using System.Collections.Generic;

namespace ipScan.Base
{
    interface ISearchTask<T, TSub>
    {        
        bool                                isRunning { get; }        
        bool                                wasStopped { get; }
        int                                 taskId { get; }
        
        BufferedResult<T>                   buffer { get; }        
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
