using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;
using System.Collections.Concurrent;

namespace ipScan.Base
{
    abstract class SearchTask<T, TSub> : ISearchTask<T, TSub>
    {
        public object                       Locker { get; } = new object();

        public bool                         IsRunning { get; protected set; }        
        public bool                         WasStopped { get; protected set; }
        protected CancellationToken         cancellationToken { get; set; }
        public int                          TaskId { get; private set; }        

        protected ITasksChecking            TasksChecking { get; set; }
        public BufferedResult<T>            Buffer { get; private set; }        
        public ConcurrentDictionary<TSub, bool> SubTaskStates { get; private set; }
        public ConcurrentDictionary<uint, uint> ProgressDict { get; private set; }
        public uint                         FirstIPAddress { get; private set; }
        protected uint                      _currentPosition;
        public uint                         CurrentPosition
        {
            get { return _currentPosition; }
            protected set { _currentPosition = value; }
        }
        public uint                         Count { get; set; }
        public uint                         Remaind
        {
            get
            {
                return (FirstIPAddress + Count - 1 - CurrentPosition);
            }
        }
        protected int                       _progress;
        public int                          Progress
        {
            get { return _progress; }
            protected set { _progress = value; }
        }
        protected int                       _timeOut { get; set; }
        public bool                         IsPaused { get; private set; }
        private Action<T>                   _bufferResultAddLine { get; set; }    
        protected ComputerInfo              ComputerInfo { get; set; }

        public SearchTask(int TaskId, uint firstIpAddress, uint Count, Action<T> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ITasksChecking CheckTasks)
        {
            Buffer = new BufferedResult<T>();
            SubTaskStates = new ConcurrentDictionary<TSub, bool>();
            ProgressDict = new ConcurrentDictionary<uint, uint>();
            this.TaskId = TaskId;            
            TasksChecking = CheckTasks;
            FirstIPAddress = firstIpAddress;
            this.Count = Count;
            CurrentPosition = FirstIPAddress;
            _timeOut = TimeOut;
            _bufferResultAddLine = BufferResultAddLine;
            IsPaused = false;
            IsRunning = false;            
            WasStopped = false;
            cancellationToken = CancellationToken;
            Progress = 0;
        }

        protected void TSub_BeforeChanged(object sender, EventArgs e)
        {
            try
            {
                if (SubTaskStates.ContainsKey((TSub)sender))
                {
                    SubTaskStates[(TSub)sender] = true;
                }
                else
                {
                    SubTaskStates.TryAdd((TSub)sender, true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }
        protected void TSub_AfterChanged(object sender, EventArgs e)
        {
            try
            {
                if (SubTaskStates.ContainsKey((TSub)sender))
                    SubTaskStates[(TSub)sender] = false;
            }
            catch (Exception)
            {
                
            }
        }

        public void Init(uint firstIPAddress, uint count)
        {
            FirstIPAddress = CurrentPosition = firstIPAddress;
            Count = count;
        }
        
        protected abstract void Search();

        public void Start()
        {
            IsRunning = true;
            Search();
        }
        public abstract void Stop();
        
        public void Pause(bool IsPaused)
        {
            this.IsPaused = IsPaused;
        }
    }
}
