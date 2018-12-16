using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;
using System.Collections.Concurrent;
using NLog;

namespace ipScan.Base
{
    abstract class SearchTask<T, TSub> : ISearchTask<T, TSub>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected const int PAUSE_SLEEP_TIME = 100;

        protected object                    _locker { get; } = new object();

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
        protected int                       _timeOut;
        private bool                        _isPaused;
        public bool                         IsPaused
        {
            get
            {
                return _isPaused;
            }
            private set
            {
                _isPaused = value;
                if (value)
                {
                    _logger.Trace(string.Format("Task [{0}] was paused", TaskId));
                }
                else
                {
                    _logger.Trace(string.Format("Task [{0}] was unpaused", TaskId));
                }
            }
        }
        private Action<T>                   _bufferResultAddLine;
        protected ComputerInfo              _computerInfo;

        public SearchTask(int TaskId, uint firstIpAddress, uint Count, Action<T> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ITasksChecking CheckTasks)
        {
            this.Buffer = new BufferedResult<T>();
            this.SubTaskStates = new ConcurrentDictionary<TSub, bool>();
            this.ProgressDict = new ConcurrentDictionary<uint, uint>();
            this.TaskId = TaskId;            
            this.TasksChecking = CheckTasks;
            this.FirstIPAddress = firstIpAddress;
            this.Count = Count;
            this.CurrentPosition = FirstIPAddress;
            this._timeOut = TimeOut;
            this._bufferResultAddLine = BufferResultAddLine;
            this.IsPaused = false;
            this.IsRunning = false;            
            this.WasStopped = false;
            this.cancellationToken = CancellationToken;
            this.Progress = 0;
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
                _logger.Error(ex);
            }
        }
        protected void TSub_AfterChanged(object sender, EventArgs e)
        {
            try
            {
                if (SubTaskStates.ContainsKey((TSub)sender))
                    SubTaskStates[(TSub)sender] = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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
