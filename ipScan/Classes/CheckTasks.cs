using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ipScan.Classes
{
    class CheckTasks
    {
        private static readonly object lockObject = new object();
        public Task[] myTasks { get; private set; }
        public SearchTask[] mySearchTasks { get; private set; }
        private Action<bool> startButtonEnable { get; set; }
        private Action<bool> stopButtonEnable { get; set; }
        private Action<object> resultAppendBuffer { get; set; }
        private BufferResult bufferResult { get; set; }        
        private Action<object> disposeTasks { get; set; }
        Action<int, int, ListIPInfo, ListIPInfo, int, ListIPInfo, TimeSpan, TimeSpan> setProgress { get; set; }
        private int IPListCount { get; set; }
        private DateTime timeStart { get; set; }
        public bool isRunning { get; private set; }
        public bool isPaused { get; private set; }
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
            Task[] MyTasks,
            SearchTask[] MySearchTasks,
            Action<bool> StartButtonEnable,
            Action<bool> StopButtonEnable,
            Action<object> ResultAppendBuffer,
            Action<object> DisposeTasks,
            Action<int, int, ListIPInfo, ListIPInfo, int, ListIPInfo, TimeSpan, TimeSpan> SetProgress,
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
            isRunning = true;
            try
            {
                System.GC.Collect();
                bool TasksAreRunning;
                TasksAreRunning = false;
                int OkRemaind = 50;
                int maxRemaind = OkRemaind;
                do
                {
                    if (isPaused)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(1000);

                        TasksAreRunning = false;
                        int progress = 0;
                        int thread4IpCount = 0;
                        int thread4HostNameCount = 0;
                        List<int> ListIp = new List<int>();
                        List<int> ListHostNames = new List<int>();
                        ListIPInfo IpArePassed = new ListIPInfo();
                        ListIPInfo IpAreFound = new ListIPInfo();
                        ListIPInfo IpAreLooking4HostName = new ListIPInfo();
                        int i = 0;
                        while (i < mySearchTasks.Count())
                        {
                            try
                            {
                                bool SubTasksAreRunning = false;
                                try
                                {                                    
                                    foreach (IPAddress key in mySearchTasks[i].isLooking4HostNames.Keys)
                                    {
                                        bool subIsRunning = mySearchTasks[i].isLooking4HostNames[key];
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
                                catch (Exception)
                                {
                                }

                                if (mySearchTasks[i].isRunning)
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
                                            //myTasks[i] = new Task(mySearchTasks[i].Start);
                                            //myTasks[i].Start();
                                        }
                                    }
                                }


                                TasksAreRunning = TasksAreRunning || !myTasks[i].IsCompleted || SubTasksAreRunning;
                                
                                ListIPInfo buffer = mySearchTasks[i].buffer.getBuffer();
                                if (buffer.Count() > 0)
                                {
                                    bufferResult.AddLines(buffer);
                                    IpAreFound.AddRange(buffer);
                                }
                                
                                buffer = mySearchTasks[i].IpArePassed.getBuffer();
                                if (buffer.Count() > 0)
                                {
                                    IpArePassed.AddRange(buffer);
                                }
                                
                                progress += mySearchTasks[i].progress;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                            }
                            i++;
                        }
                        if (TasksAreRunning)
                        {
                            isRunning = false;
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
                        catch (Exception)
                        {
                            timePassed = TimeSpan.MinValue;
                            timeLeft = TimeSpan.MinValue;
                        }
                        setProgress(progress, thread4IpCount, IpArePassed, IpAreFound, thread4HostNameCount, IpAreLooking4HostName, timePassed, timeLeft);
                    }
                } while ((TasksAreRunning || isRunning) && !isStopped);
                disposeTasks(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("Усі задачі зупинено");
            startButtonEnable(true);
            stopButtonEnable(false);
        }
    }
}
