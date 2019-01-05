using ipScan.Base.IP;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ipScan.Base
{
    class TasksChecking<T, TSub> : ITasksChecking
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private object _locker = new object();

        private bool wasStopped { get; set; }
        private static readonly object          lockObject = new object();
        private const uint                      MIN_REMAIND  = 4;

        private List<ISearchTask<T, TSub>>      _mySearchTasks { get; set; }
        public bool                             MySearchTasksStartedAll { get; protected set; }

        private Action<bool>                    startButtonEnable { get; set; }
        private Action<bool>                    stopButtonEnable { get; set; }
        private Action<object>                  resultAppendBuffer { get; set; }
        private BufferedResult<T>               bufferResult { get; set; }        
        private Action<object>                  disposeTasks { get; set; }

        private Action<IProgressData> setProgress { get; set; }

        private uint                            IPListCount { get; set; }
        private DateTime                        timeStart { get; set; }
        public DateTime                         LastTime { get; private set; }
        public TimeSpan                         loopTime { get; protected set; }
        public int                              SleepTime { get; set; }
        public bool                             IsStarting { get; set; }
        private bool _isPaused;
        protected bool                          IsPaused
        {
            get
            {
                return _isPaused;
            }

            set
            {
                _isPaused = value;
                foreach (var item in _mySearchTasks)
                {
                    item.Pause(value);
                }
            }
        }
        private bool                            isStopped { get; set; }
        private bool                            isResultOutputBlocked { get; set; }

        public int                              TasksCount { get; private set; }
        private long                            _progressRemaind;

        public TasksChecking(
            List<ISearchTask<T, TSub>> MySearchTasks,
            Action<bool> StartButtonEnable,
            Action<bool> StopButtonEnable,
            Action<object> ResultAppendBuffer,
            Action<object> DisposeTasks,
            Action<IProgressData> SetProgress,
            uint ipListCount,
            BufferedResult<T> BufferResult)
        {
            this._mySearchTasks = MySearchTasks;
            startButtonEnable = StartButtonEnable;
            stopButtonEnable = StopButtonEnable;
            resultAppendBuffer = ResultAppendBuffer;
            setProgress = SetProgress;
            this.IPListCount = ipListCount;
            timeStart = DateTime.Now;
            LastTime = DateTime.Now;
            loopTime = TimeSpan.FromMilliseconds(0);
            bufferResult = BufferResult;
            disposeTasks = DisposeTasks;
            isStopped = false;
            SleepTime = 1000;
            MySearchTasksStartedAll = false;
            IsStarting = true;
        }
        
        public void Check()
        {            
            TimeSpan timePassed = TimeSpan.MinValue;
            try
            {
                bool TasksAreRunning = false;
                bool TasksAreCompleted = false;                
                LastTime = DateTime.Now;
                Stopwatch stopwatch = new Stopwatch();
                do
                {
                    Thread.Sleep(SleepTime);
                    if (!IsPaused)                    
                    {
                        bool mySearchTasksStartedAll = true;
                        TasksAreRunning = false;
                        int progress = 0;
                        int tasksCount = 0;
                        int subTasksCount = 0;

                        #region mySearchTasks Handling


                        for (int i = 0; i < _mySearchTasks.Count; i++)
                        {
                            try
                            {
                                bool SubTasksAreRunning = false;
                                var mySearchTask = _mySearchTasks[i];

                                #region Get Statistics
                                try
                                {
                                    if (mySearchTask != null && mySearchTask.SubTaskStates != null)
                                    {
                                        Dictionary<TSub, bool> subTaskStates = new Dictionary<TSub, bool>(mySearchTask.SubTaskStates);
                                        bool value = false;
                                        foreach (TSub key in subTaskStates.Keys)
                                        {
                                            bool subIsRunning = subTaskStates[key];
                                            SubTasksAreRunning |= subIsRunning;
                                            if (!subIsRunning)
                                            {
                                                mySearchTask.SubTaskStates.TryRemove(key, out value);
                                            }
                                            else
                                            {
                                                subTasksCount++;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex);
                                }

                                #endregion

                                if (mySearchTask != null)
                                {
                                    if (mySearchTask.IsRunning && !mySearchTask.Waiting4TasksChecking)
                                    {
                                        tasksCount++;
                                    }
                                    mySearchTasksStartedAll = mySearchTasksStartedAll && (mySearchTask.IsRunning || mySearchTask.WasStopped || mySearchTask.Progress > 0);
                                }

                                TasksAreRunning = TasksAreRunning || mySearchTask.MyTask != null && !mySearchTask.MyTask.IsCompleted || SubTasksAreRunning;
                                TasksAreCompleted = TasksAreCompleted || mySearchTask.MyTask != null && mySearchTask.MyTask.IsCompleted;
                                bufferResult.AddLines(mySearchTask.Buffer.getBuffer());

                                progress += mySearchTask.Progress;
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex);
                            }
                        }


                        #endregion

                        MySearchTasksStartedAll = mySearchTasksStartedAll;

                        #region Calculate Time

                        if (!isResultOutputBlocked)
                        {
                            resultAppendBuffer(null);
                        }                                               
                        
                        DateTime Now = DateTime.Now;
                        
                        TimeSpan timeLeft;
                        try
                        {
                            timePassed = Now - timeStart;
                            if (progress == 0)
                            {
                                timeLeft = TimeSpan.MaxValue;
                            }
                            else
                            {
                                timeLeft = TimeSpan.FromMilliseconds((IPListCount - progress) * (timePassed.TotalMilliseconds / progress));
                            }
                        }
                        catch (Exception ex)
                        {
                            timePassed = TimeSpan.MinValue;
                            timeLeft = TimeSpan.MinValue;
                            _logger.Error(ex);
                        }

                        #endregion

                        this.TasksCount = tasksCount;
                        this._progressRemaind = IPListCount - progress;
                        
                        setProgress(new ProgressData()
                        {
                            Progress = progress,
                            TasksCount = tasksCount,
                            SubTasksCount = subTasksCount,
                            TimePassed = timePassed,
                            TimeLeft = timeLeft,
                            PauseTime = (int)loopTime.TotalMilliseconds                            
                        } );
                    }
                    loopTime = DateTime.Now - LastTime;
                    LastTime = DateTime.Now;
                } while ((TasksAreRunning || IsStarting) && !isStopped);
                disposeTasks(null);                
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            _logger.Info(string.Concat("All done, time: [", (timePassed == TimeSpan.MinValue ? "" : timePassed.ToString()), "]"));
            startButtonEnable(true);
            stopButtonEnable(false);
        }

        private int GetMaxRemaindTaskIndex()
        {
            var maxRemaind = MIN_REMAIND;
            int result = -1;
            for (int i = 0; i < _mySearchTasks.Count; i++)
            {
                var searchTask = _mySearchTasks[i];
                if (searchTask.IsRunning && !searchTask.IsBlocked && searchTask.Remaind > maxRemaind)
                {
                    maxRemaind = searchTask.Remaind;
                    result = searchTask.TaskId;
                }
            }            
            return result;
        }

        public bool ChangeSearchTaskRange(int Index)
        {
            bool result = false;

            if (TasksCount <= _progressRemaind)
            {
                var mySearchTask = _mySearchTasks[Index];
                var taskIndex = GetMaxRemaindTaskIndex();
                if (taskIndex >= 0)
                {
                    var mySearchTaskMaxRemind = _mySearchTasks[taskIndex];                
                    if (mySearchTaskMaxRemind.BlockWith(Index))
                    {
                        mySearchTaskMaxRemind.Pause(true);                    
                        uint remaind = mySearchTaskMaxRemind.Remaind;
                        if (remaind >= MIN_REMAIND)
                        {
                            uint oldCount = mySearchTaskMaxRemind.Count;
                            uint index = mySearchTaskMaxRemind.FirstIPAddress;
                            uint newRemaind = remaind / 2;
                            uint newCount = oldCount - newRemaind;

                            mySearchTaskMaxRemind.Count = newCount;

                            mySearchTask.Init(index + newCount, newRemaind);
                            _logger.Trace(string.Concat(
                                "Task [", mySearchTaskMaxRemind.TaskId, "] ",
                                "FirstIP: ", mySearchTaskMaxRemind.FirstIPAddress, ", ",
                                "LastIP: ", mySearchTaskMaxRemind.FirstIPAddress + oldCount - 1, ", ",
                                "Remind: ", remaind, ", ",
                                "Count: ", oldCount, ", ",
                                "New LastIP: ", mySearchTaskMaxRemind.FirstIPAddress + mySearchTaskMaxRemind.Count - 1, " >>> ",
                                "[", mySearchTask.TaskId, "] ",
                                "FirstIP: ", mySearchTask.FirstIPAddress, ", ",
                                "LastIP: ", mySearchTask.FirstIPAddress + mySearchTask.Count - 1
                            ));
                            result = true;
                        }
                        mySearchTaskMaxRemind.Pause(false);
                        mySearchTaskMaxRemind.UnBlock();
                    }
                }                        
            }


            return result;
        }

        public void Stop()
        {
            isStopped = true;
        }

        public bool Pause()
        {
            IsPaused = !IsPaused;
            return IsPaused;
        }

        public void Pause(bool IsPaused)
        {
            this.IsPaused = IsPaused;            
        }
    }
}
