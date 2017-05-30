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
        public Dictionary<int, int>         Progress { get; private set; }
        protected List<TSub>                mainList { get; set; }
        public int                          index { get; private set; }
        protected int                       _currentPosition;
        public int                          currentPosition
        {
            get { return _currentPosition; }
            protected set { _currentPosition = value; }
        }
        public int                          count { get; set; }
        public int                          remaind
        {
            get
            {
                return (index + count - 1 - currentPosition);
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

        public SearchTask(int TaskId, List<TSub> TList, int Index, int Count, Action<T> BufferResultAddLine, int TimeOut, int maxTaskCountLimit, CancellationToken CancellationToken, ICheckSearchTask CheckTasks)
        {
            Buffer = new BufferedResult<T>();
            SubTaskStates = new Dictionary<TSub, bool>();
            Progress = new Dictionary<int, int>();
            taskId = TaskId;            
            checkTasks = CheckTasks;
            this.mainList = TList;
            index = Index;
            count = Count;
            currentPosition = index;
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
                SubTaskStates.Add((TSub)sender, true);
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

        public void Init(int Index, int Count)
        {
            index = currentPosition = Index;
            count = Count;
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
