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

        private Action<bool>                    startButtonEnable { get; set; }
        private Action<bool>                    stopButtonEnable { get; set; }
        private Action<object>                  resultAppendBuffer { get; set; }
        private BufferedResult<T>               bufferResult { get; set; }        
        private Action<object>                  disposeTasks { get; set; }

        private Action<int, int, int, TimeSpan, TimeSpan, int> setProgress { get; set; }

        private int                             TListCount { get; set; }
        private DateTime                        timeStart { get; set; }
        public DateTime                         LastTime { get; private set; }
        private TimeSpan                        loopTime { get; set; }
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
        }
        
        public void Check()
        {
            isStarting = true;
            try
            {
                GC.Collect();
                bool TasksAreRunning = false;
                bool TasksAreCompleted = false;
                int maxRemaind = OkRemaind;
                LastTime = DateTime.Now;
                int mySearchTasksPauseTime = 1000;
                do
                {
                    if (isPaused)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(mySearchTasksPauseTime);                        

                        loopTime = DateTime.Now - LastTime;
                        LastTime = DateTime.Now;
                        mySearchTasksPauseTime += (int)(1000 - loopTime.TotalMilliseconds);
                        if (mySearchTasksPauseTime < 0)
                        {
                            mySearchTasksPauseTime = 0;
                        }

                        TasksAreRunning = false;
                        int progress = 0;
                        int TasksCount = 0;
                        int subTasksCount = 0;
                        
                        int i = 0;
                        while (i < mySearchTasks.Count())
                        {
                            try
                            {
                                bool SubTasksAreRunning = false;
                                try
                                {
                                    if (mySearchTasks[i] != null && mySearchTasks[i].SubTaskStates != null)
                                    {
                                        Dictionary<TSub, bool> subTaskStates = new Dictionary<TSub, bool>(mySearchTasks[i].SubTaskStates);
                                        foreach (TSub key in subTaskStates.Keys)
                                        {
                                            bool subIsRunning = subTaskStates[key];
                                            SubTasksAreRunning |= subIsRunning;
                                            if (!subIsRunning)
                                            {
                                                mySearchTasks[i].SubTaskStates.Remove(key);
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

                                if (mySearchTasks[i] != null && mySearchTasks[i].isRunning)
                                {
                                    TasksCount++;
                                    TasksCount += mySearchTasks[i].WorkingTaskCount;
                                    /*
                                    try
                                    {
                                        List<Task> tasks = mySearchTasks[i].Tasks;
                                        if (tasks != null)
                                        {

                                            foreach (Task task in tasks)
                                            {
                                                if (task.IsCompleted)
                                                {
                                                    tasks.Remove(task);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
                                    }
                                    */
                                }
                                else
                                {
                                    if (myTasks[i].IsCompleted && maxRemaind >= OkRemaind)                                    
                                    {
                                        maxRemaind = 0;
                                        int taskIndex = -1;
                                        for (int j = 0; j < mySearchTasks.Count(); j++)
                                        {
                                            if (mySearchTasks[j].isRunning)
                                            {
                                                int taskRemaind = mySearchTasks[j].remaind;
                                                if (taskRemaind > maxRemaind)
                                                {
                                                    maxRemaind = taskRemaind;
                                                    taskIndex = j;
                                                }
                                            }
                                        }
                                        if (taskIndex >= 0 && maxRemaind >= OkRemaind)
                                        {
                                            mySearchTasks[taskIndex].Pause(true);
                                            while (!mySearchTasks[taskIndex].isPaused)
                                            {
                                                Thread.Sleep(1);
                                            }
                                            int remaind = mySearchTasks[taskIndex].remaind;
                                            int count = mySearchTasks[taskIndex].count;
                                            int index = mySearchTasks[taskIndex].index;
                                            int newRemaind = remaind / 2;
                                            int newCount = count - newRemaind;

                                            mySearchTasks[taskIndex].count = newCount;
                                            mySearchTasks[taskIndex].Pause(false);

                                            mySearchTasks[i].Init(index + newCount, newRemaind);
                                            myTasks[i] = Task.Factory.StartNew(mySearchTasks[i].Start);
                                            Console.WriteLine(i.ToString() + " is joined to " + taskIndex.ToString());
                                        }
                                    }
                                }


                                TasksAreRunning = TasksAreRunning || !myTasks[i].IsCompleted || SubTasksAreRunning;
                                TasksAreCompleted = TasksAreCompleted || myTasks[i].IsCompleted;
                                bufferResult.AddLines(mySearchTasks[i].buffer.getBuffer());
                                                                
                                progress += mySearchTasks[i].progress;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.StackTrace);
                            }
                            i++;
                        }
                        
                        if (TasksAreRunning || TasksAreCompleted)
                        {
                            isStarting = false;
                        }

                        if (!isResultOutputBlocked)
                        {
                            resultAppendBuffer(null);
                        }

                        DateTime Now = DateTime.Now;
                        TimeSpan timePassed;
                        TimeSpan timeLeft;
                        try
                        {
                            timePassed = Now - timeStart;
                            timeLeft = TimeSpan.FromMilliseconds((TListCount - progress) * (timePassed.TotalMilliseconds / progress));
                        }
                        catch (Exception ex)
                        {
                            timePassed = TimeSpan.MinValue;
                            timeLeft = TimeSpan.MinValue;
                            Debug.WriteLine(ex.StackTrace);
                        }
                        setProgress(progress, TasksCount, subTasksCount, timePassed, timeLeft, (int)loopTime.TotalMilliseconds);
                        
                    }
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
