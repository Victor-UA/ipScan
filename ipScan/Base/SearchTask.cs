using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;
using System.Collections.Concurrent;

namespace ipScan.Base
{
    abstract class SearchTask<T, TSub> : ISearchTask<T, TSub>
    {
        public object                       Locker { get; } = new object();

        public bool                         isRunning { get; protected set; }        
        public bool                         wasStopped { get; protected set; }
        protected CancellationToken         cancellationToken { get; set; }
        public int                          taskId { get; private set; }        
        public Dictionary<object, Task>     Tasks { get; protected set; }        

        protected ITasksChecking            TasksChecking { get; set; }
        public BufferedResult<T>            Buffer { get; private set; }        
        public ConcurrentDictionary<TSub, bool> SubTaskStates { get; private set; }
        public Dictionary<uint, uint>       Progress { get; private set; }
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
        public int                          progress
        {
            get { return _progress; }
            protected set { _progress = value; }
        }
        protected int                       timeOut { get; set; }
        public bool                         isPaused { get; private set; }
        private Action<T>                   bufferResultAddLine { get; set; }    
        protected ComputerInfo              ComputerInfo { get; set; }

        public SearchTask(int TaskId, uint firstIpAddress, uint Count, Action<T> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ITasksChecking CheckTasks)
        {
            Buffer = new BufferedResult<T>();
            SubTaskStates = new ConcurrentDictionary<TSub, bool>();
            Progress = new Dictionary<uint, uint>();
            taskId = TaskId;            
            TasksChecking = CheckTasks;
            FirstIPAddress = firstIpAddress;
            this.Count = Count;
            CurrentPosition = FirstIPAddress;
            timeOut = TimeOut;
            bufferResultAddLine = BufferResultAddLine;
            isPaused = false;
            isRunning = false;            
            wasStopped = false;
            cancellationToken = CancellationToken;
            progress = 0;
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
            isRunning = true;
            Search();
        }
        public abstract void Stop();
        
        public void Pause(bool IsPaused)
        {
            isPaused = IsPaused;
        }
    }
}
