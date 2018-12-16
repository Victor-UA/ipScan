using System.Collections.Concurrent;

namespace ipScan.Base
{
    interface ISearchTask<T, TSub>
    {
        bool                                IsRunning { get; }        
        bool                                WasStopped { get; }
        int                                 TaskId { get; }

        BufferedResult<T>                   Buffer { get; }        
        ConcurrentDictionary<TSub, bool>    SubTaskStates { get; }
        ConcurrentDictionary<uint, uint>    ProgressDict { get; }
        uint                                FirstIPAddress { get; }
        uint                                CurrentPosition { get; }
        uint                                Count { get; set; }
        uint                                Remaind { get; }
                
        int                                 Progress { get; }
        bool                                IsPaused { get; }

        void                                Init(uint firstIPAddress, uint count);

        void                                Start();
        void                                Stop();
        void                                Pause(bool IsPaused);        
    }
}
