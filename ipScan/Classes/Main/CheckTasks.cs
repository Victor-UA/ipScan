using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ipScan.Classes.Main
{
    class CheckTasks
    {
        private static readonly object lockObject = new object();
        private int OkRemaind { get; } = 4;
        public List<Task> myTasks { get; private set; }
        public List<SearchTask> mySearchTasks { get; private set; }
        private Action<bool> startButtonEnable { get; set; }
        private Action<bool> stopButtonEnable { get; set; }
        private Action<object> resultAppendBuffer { get; set; }
        private BufferResult bufferResult { get; set; }        
        private Action<object> disposeTasks { get; set; }
        Action<int, int, int, TimeSpan, TimeSpan, int> setProgress { get; set; }
        private int IPListCount { get; set; }
        private DateTime timeStart { get; set; }
        public DateTime lastTime { get; private set; }
        public TimeSpan loopTime { get; private set; }
        public bool isStarting { get; private set; }
        public bool isPaused { get; private set; }
        public bool firstPause { get; set; }
        public bool isStopped { get; private set; }
        private bool isResultOutputBlocked { get; set; }
        public void Stop()
        {
            isStopped = true;
        }
        public void Pause()
        {
            isPaused = !isPaused;
        }

        public CheckTasks(
            List<Task> MyTasks,
            List<SearchTask> MySearchTasks,
            Action<bool> StartButtonEnable,
            Action<bool> StopButtonEnable,
            Action<object> ResultAppendBuffer,
            Action<object> DisposeTasks,
            Action<int, int, int, TimeSpan, TimeSpan, int> SetProgress,
            int IPListCount,
            BufferResult BufferResult)
        {
            myTasks = MyTasks;
            mySearchTasks = MySearchTasks;
            startButtonEnable = StartButtonEnable;
            stopButtonEnable = StopButtonEnable;
            resultAppendBuffer = ResultAppendBuffer;
            setProgress = SetProgress;
            this.IPListCount = IPListCount;
            timeStart = DateTime.Now;
            lastTime = DateTime.Now;
            loopTime = TimeSpan.FromMilliseconds(0);
            bufferResult = BufferResult;
            disposeTasks = DisposeTasks;
            isStopped = false;
        }

        public void BlockResultOutput (bool IsResultOutputBlocked)
        {
            isResultOutputBlocked = IsResultOutputBlocked;
        }
        public void Check()
        {
            isStarting = true;
            try
            {
                System.GC.Collect();
                bool TasksAreRunning;
                TasksAreRunning = false;                
                int maxRemaind = OkRemaind;
                lastTime = DateTime.Now;
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

                        loopTime = DateTime.Now - lastTime;
                        lastTime = DateTime.Now;
                        mySearchTasksPauseTime += (int)(1000 - loopTime.TotalMilliseconds);
                        if (mySearchTasksPauseTime < 0)
                        {
                            mySearchTasksPauseTime = 0;
                        }

                        TasksAreRunning = false;
                        int progress = 0;
                        int thread4IpCount = 0;
                        int thread4HostNameCount = 0;
                        List<int> ListIp = new List<int>();
                        List<int> ListHostNames = new List<int>();                                              
                        int i = 0;
                        while (i < mySearchTasks.Count())
                        {
                            //mySearchTasks[i].pauseTime = mySearchTasksPauseTime;                            
                            try
                            {
                                bool SubTasksAreRunning = false;
                                try
                                {
                                    if (mySearchTasks[i] != null && mySearchTasks[i].isLooking4HostNames != null)
                                    {
                                        Dictionary<IPAddress, bool> isLooking4HostNames = new Dictionary<IPAddress, bool>(mySearchTasks[i].isLooking4HostNames);
                                        foreach (IPAddress key in isLooking4HostNames.Keys)
                                        {
                                            bool subIsRunning = isLooking4HostNames[key];
                                            SubTasksAreRunning |= subIsRunning;
                                            if (!subIsRunning)
                                            {
                                                mySearchTasks[i].isLooking4HostNames.Remove(key);
                                            }
                                            else
                                            {
                                                thread4HostNameCount++;
                                                //IpAreLooking4HostName.Add(new IPInfo(key));
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
                                    thread4IpCount++;                                    
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
                                            //myTasks[i] = NewTask(mySearchTasks[i].Start);                                            
                                            //myTasks[i] = new Task(mySearchTasks[i].Start);
                                            //myTasks[i].Start();
                                        }
                                    }
                                }


                                TasksAreRunning = TasksAreRunning || !myTasks[i].IsCompleted || SubTasksAreRunning;
                                bufferResult.AddLines(mySearchTasks[i].buffer.getBuffer());
                                /*
                                ListIPInfo buffer = mySearchTasks[i].buffer.getBuffer();
                                if (buffer.Count() > 0)
                                {
                                    bufferResult.AddLines(buffer);
                                }
                                */
                                progress += mySearchTasks[i].progress;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.StackTrace);
                            }
                            i++;
                        }
                        
                        if (TasksAreRunning)
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
                            timeLeft = TimeSpan.FromMilliseconds((IPListCount - progress) * (timePassed.TotalMilliseconds / progress));
                        }
                        catch (Exception ex)
                        {
                            timePassed = TimeSpan.MinValue;
                            timeLeft = TimeSpan.MinValue;
                            Debug.WriteLine(ex.StackTrace);
                        }
                        setProgress(progress, thread4IpCount, thread4HostNameCount, timePassed, timeLeft, (int)loopTime.TotalMilliseconds);
                        
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
    }
}
