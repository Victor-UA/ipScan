using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;

namespace ipScan.Base
{
    abstract class SearchTask<T, TSub> : ISearchTask<T, TSub>
    {
        public object                       Locker { get; } = new object();

        public bool                         isRunning { get; protected set; }        
        public bool                         wasStopped { get; protected set; }
        protected CancellationToken         cancellationToken { get; set; }
        public int                          taskId { get; private set; }
        public int                          _WorkingTaskCount;
        public int                          WorkingTaskCount
        {
            get { return _WorkingTaskCount; }
            protected set { _WorkingTaskCount = value; }
        }
        public Dictionary<object, Task>     Tasks { get; protected set; }
        public int                          MaxTaskCountLimit { get; set; }

        protected ICheckSearchTask          checkTasks { get; set; }
        public BufferedResult<T>            Buffer { get; private set; }        
        public Dictionary<TSub, bool>       SubTaskStates { get; private set; }
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
        protected uint                      _progress;
        public uint                         progress
        {
            get { return _progress; }
            protected set { _progress = value; }
        }
        protected int                       timeOut { get; set; }
        public bool                         isPaused { get; private set; }
        private Action<T>                   bufferResultAddLine { get; set; }    
        protected ComputerInfo              ComputerInfo { get; set; }

        public SearchTask(int TaskId, uint firstIpAddress, uint Count, Action<T> BufferResultAddLine, int TimeOut, int maxTaskCountLimit, CancellationToken CancellationToken, ICheckSearchTask CheckTasks)
        {
            Buffer = new BufferedResult<T>();
            SubTaskStates = new Dictionary<TSub, bool>();
            Progress = new Dictionary<uint, uint>();
            taskId = TaskId;            
            checkTasks = CheckTasks;
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
            MaxTaskCountLimit = maxTaskCountLimit;
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
                    SubTaskStates.Add((TSub)sender, true);
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
