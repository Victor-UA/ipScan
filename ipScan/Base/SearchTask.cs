using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace ipScan.Base
{
    abstract class SearchTask<T, TSub> : ISearchTask<T, TSub>
    {        
        public bool                         isRunning { get; protected set; }        
        public bool                         wasStopped { get; protected set; }
        protected CancellationToken         cancellationToken { get; set; }
        public int                          taskId { get; private set; }
        
        protected ICheckSearchTask          checkTasks { get; set; }
        public BufferedResult<T>            buffer { get; private set; }        
        public Dictionary<TSub, bool>       SubTaskStates { get; private set; }
        public Dictionary<int, int>         Progress { get; private set; }
        protected List<TSub>                mainList { get; set; }
        public int                          index { get; private set; }
        public int                          currentPosition { get; protected set; }
        public int                          count { get; set; }
        public int                          remaind
        {
            get
            {
                return (index + count - 1 - currentPosition);
            }
        }
        public int                          progress { get; protected set; }
        protected int                       timeOut { get; set; }
        public bool                         isPaused { get; private set; }
        private Action<T>                   bufferResultAddLine { get; set; }        

        public SearchTask(int TaskId, List<TSub> TList, int Index, int Count, Action<T> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ICheckSearchTask CheckTasks)
        {
            buffer = new BufferedResult<T>();
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
                SubTaskStates[(TSub)sender] = false;
            }
            catch (Exception) { }
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
