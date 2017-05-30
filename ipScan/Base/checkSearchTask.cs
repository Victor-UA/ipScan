using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ipScan.Base
{
    class CheckSearchTask<T, TSub> : ICheckSearchTask
    {
        private bool isRunning { get; set; }
        private bool wasStopped { get; set; }
        private static readonly object          lockObject = new object();
        private int                             OkRemaind { get; } = 4;

        private List<Task>                      myTasks { get; set; }
        private List<ISearchTask<T, TSub>>      mySearchTasks { get; set; }
        public bool                             MySearchTasksStartedAll { get; protected set; }

        private Action<bool>                    startButtonEnable { get; set; }
        private Action<bool>                    stopButtonEnable { get; set; }
        private Action<object>                  resultAppendBuffer { get; set; }
        private BufferedResult<T>               bufferResult { get; set; }        
        private Action<object>                  disposeTasks { get; set; }

        private Action<int, int, int, TimeSpan, TimeSpan, int> setProgress { get; set; }

        private int                             TListCount { get; set; }
        private DateTime                        timeStart { get; set; }
        public DateTime                         LastTime { get; private set; }
        public TimeSpan                         loopTime { get; protected set; }
        public int                              SleepTime { get; set; }
        private bool                            isStarting { get; set; }
        private bool                            isPaused { get; set; }
        private bool                            isStopped { get; set; }
        private bool                            isResultOutputBlocked { get; set; }


        public CheckSearchTask(
            List<Task> MyTasks,
            List<ISearchTask<T, TSub>> MySearchTasks,
            Action<bool> StartButtonEnable,
            Action<bool> StopButtonEnable,
            Action<object> ResultAppendBuffer,
            Action<object> DisposeTasks,
            Action<int, int, int, TimeSpan, TimeSpan, int> SetProgress,
            int TListCount,
            BufferedResult<T> BufferResult)
        {
            myTasks = MyTasks;
            mySearchTasks = MySearchTasks;
            startButtonEnable = StartButtonEnable;
            stopButtonEnable = StopButtonEnable;
            resultAppendBuffer = ResultAppendBuffer;
            setProgress = SetProgress;
            this.TListCount = TListCount;
            timeStart = DateTime.Now;
            LastTime = DateTime.Now;
            loopTime = TimeSpan.FromMilliseconds(0);
            bufferResult = BufferResult;
            disposeTasks = DisposeTasks;
            isStopped = false;
            SleepTime = 1000;
            MySearchTasksStartedAll = false;
        }
        
        public void Check()
        {
            isStarting = true;
            try
            {
                GC.Collect();
                bool TasksAreRunning = false;
                bool TasksAreCompleted = false;
                int mySearchTasksCountLimit = 50000;
                bool mySearchTasksOutOfCountLimit_Paused = false;
                int maxRemaind = OkRemaind;
                LastTime = DateTime.Now;
                do
                {                    
                    if (isPaused)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(SleepTime);   
                        
                        SleepTime += (int)(1000 - loopTime.TotalMilliseconds);
                        if (SleepTime < 0)
                        {
                            SleepTime = 0;
                        }
                                               
                        bool mySearchTasksStartedAll = true;
                        TasksAreRunning = false;
                        int progress = 0;
                        int TasksCount = 0;
                        int subTasksCount = 0;
                                                
                        for (int i = 0; i < mySearchTasks.Count(); i++)
                        {
                            try
                            {
                                var mySearchTask = mySearchTasks[i];
                                bool SubTasksAreRunning = false;
                                try
                                {
                                    if (mySearchTask != null && mySearchTask.SubTaskStates != null)
                                    {
                                                                                  
                                        Dictionary<TSub, bool> subTaskStates = new Dictionary<TSub, bool>(mySearchTask.SubTaskStates);
                                        foreach (TSub key in subTaskStates.Keys)
                                        {
                                            bool subIsRunning = subTaskStates[key];
                                            SubTasksAreRunning |= subIsRunning;
                                            if (!subIsRunning)
                                            {
                                                mySearchTask.SubTaskStates.Remove(key);
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
                                    Debug.WriteLine(ex.StackTrace);
                                }

                                if (mySearchTask != null)
                                {
                                    if (mySearchTask.isRunning)
                                    {
                                        //TasksCount++;                                    
                                        TasksCount += mySearchTask.WorkingTaskCount;                                    
                                    }
                                    mySearchTasksStartedAll = mySearchTasksStartedAll && (mySearchTask.isRunning || mySearchTask.wasStopped || mySearchTask.progress > 0);
                                }
                                else
                                {
                                    if (myTasks[i].IsCompleted && maxRemaind >= OkRemaind)                                    
                                    {
                                        maxRemaind = 0;
                                        int taskIndex = -1;
                                        for (int j = 0; j < mySearchTasks.Count(); j++)
                                        {
                                            var mySearchTaskSeekRemaind = mySearchTasks[j];
                                            if (mySearchTaskSeekRemaind.isRunning)
                                            {
                                                int taskRemaind = mySearchTaskSeekRemaind.remaind;
                                                if (taskRemaind > maxRemaind)
                                                {
                                                    maxRemaind = taskRemaind;
                                                    taskIndex = j;
                                                }
                                            }
                                        }
                                        if (taskIndex >= 0 && maxRemaind >= OkRemaind)
                                        {
                                            var mySearchTaskMaxRemind = mySearchTasks[taskIndex];
                                            mySearchTaskMaxRemind.Pause(true);
                                            while (!mySearchTaskMaxRemind.isPaused)
                                            {
                                                Thread.Sleep(1);
                                            }
                                            int remaind = mySearchTaskMaxRemind.remaind;
                                            int count = mySearchTaskMaxRemind.count;
                                            int index = mySearchTaskMaxRemind.index;
                                            int newRemaind = remaind / 2;
                                            int newCount = count - newRemaind;

                                            mySearchTaskMaxRemind.count = newCount;
                                            mySearchTaskMaxRemind.Pause(false);

                                            mySearchTask.Init(index + newCount, newRemaind);
                                            myTasks[i] = Task.Factory.StartNew(mySearchTask.Start);
                                            Console.WriteLine(i.ToString() + " is joined to " + taskIndex.ToString());
                                        }
                                    }
                                }
                                
                                TasksAreRunning = TasksAreRunning || !myTasks[i].IsCompleted || SubTasksAreRunning;
                                TasksAreCompleted = TasksAreCompleted || myTasks[i].IsCompleted;
                                bufferResult.AddLines(mySearchTask.Buffer.getBuffer());
                                                                
                                progress += mySearchTask.progress;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.StackTrace);
                            }
                        }

                        MySearchTasksStartedAll = mySearchTasksStartedAll;

                        if (TasksAreRunning || TasksAreCompleted)
                        {
                            isStarting = false;
                        }

                        if (!isResultOutputBlocked)
                        {
                            resultAppendBuffer(null);
                        }
                        
                        if (TasksCount > mySearchTasksCountLimit)
                        {
                            for (int i = 0; i < mySearchTasks.Count; i++)
                            {
                                mySearchTasks[i].Pause(true);
                            }
                            mySearchTasksOutOfCountLimit_Paused = true;
                        }
                        else
                        {
                            if (mySearchTasksOutOfCountLimit_Paused)
                            {
                                for (int i = 0; i < mySearchTasks.Count; i++)
                                {
                                    mySearchTasks[i].Pause(false);
                                }
                                mySearchTasksOutOfCountLimit_Paused = false;
                            }
                        }
                        
                        DateTime Now = DateTime.Now;
                        TimeSpan timePassed;
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
                                timeLeft = TimeSpan.FromMilliseconds((TListCount - progress) * (timePassed.TotalMilliseconds / progress));
                            }
                        }
                        catch (Exception ex)
                        {
                            timePassed = TimeSpan.MinValue;
                            timeLeft = TimeSpan.MinValue;
                            Debug.WriteLine(ex.StackTrace);
                        }
                        setProgress(progress, TasksCount, subTasksCount, timePassed, timeLeft, (int)loopTime.TotalMilliseconds);
                        
                    }
                    loopTime = DateTime.Now - LastTime;
                    LastTime = DateTime.Now;
                } while ((TasksAreRunning || isStarting) && !isStopped);
                disposeTasks(null);                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("Усі задачі зупинено");
            startButtonEnable(true);
            stopButtonEnable(false);
        }

        public void Stop()
        {
            isStopped = true;
        }
        public bool Pause()
        {
            return isPaused = !isPaused;
        }        
        public bool Pause(bool IsPaused)
        {
            return isPaused = IsPaused;
        }
    }
}
