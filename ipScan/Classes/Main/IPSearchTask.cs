﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using ipScan.Base;
using ipScan.Base.IP;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ipScan.Classes.Main
{
    class IPSearchTask : SearchTask<IPInfo, IPAddress>
    {        

        public IPSearchTask(int TaskId, List<IPAddress> IPList, int Index, int Count, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ICheckSearchTask CheckTasks)
            : base(TaskId, IPList, Index, Count, BufferResultAddLine, TimeOut, CancellationToken, CheckTasks) { }
        
        protected override void Search()
        {
            Console.WriteLine(taskId + " is started");
            bool waiting4CheckTasks = false;
            int checkTasksLoopTimeMax = 60;
            int sleepTime = 100;
            Progress.Add(index, currentPosition);
            if (!wasStopped)
            {
                Tasks = new Dictionary<object, Task>();
                WorkingTaskCount = 0;
                while (isRunning && currentPosition < index + count && currentPosition < mainList.Count)
                {
                    TimeSpan checkTasksLoopTime = DateTime.Now - checkTasks.LastTime;
                    if (checkTasksLoopTime.TotalSeconds > checkTasksLoopTimeMax)
                    {
                        if (!waiting4CheckTasks)
                        {
                            Console.WriteLine(taskId.ToString() + " is waiting for checkTasks iterration. CheckTasks loop time: " + checkTasksLoopTime.TotalSeconds.ToString());
                            waiting4CheckTasks = true;
                        }
                        sleepTime += (int)(checkTasksLoopTime.TotalSeconds - checkTasksLoopTimeMax);
                        if (sleepTime > 1000)
                        {
                            sleepTime = 1000;
                        }
                    }
                    else
                    {
                        if (waiting4CheckTasks)
                        {
                            waiting4CheckTasks = false;
                            Console.WriteLine(taskId.ToString() + " resumed its work");
                        }
                        sleepTime = 100;
                    }

                    if (isPaused || waiting4CheckTasks)
                    {
                        Thread.Sleep(sleepTime);
                    }
                    else
                    {
                        if (WorkingTaskCount >= 99)
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            IPAddress address = mainList[currentPosition];
                            Task task = PingAsync(address);
                            Tasks.Add(address.ToString(), task);
                            currentPosition++;
                            Progress[index] = currentPosition;
                        }

                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Stop();
                    }
                }
            }
            //while (isRunning && currentPosition - progress != index)
            while (isRunning && WorkingTaskCount > 0)
                {
                if (cancellationToken.IsCancellationRequested)
                {
                    Stop();
                }
                Thread.Sleep(1000);
            }
            isRunning = false;
            Console.WriteLine(taskId + " is stopped");
        }

        //https://stackoverflow.com/questions/24158814/ping-sendasync-always-successful-even-if-client-is-not-pingable-in-cmd
        private async Task PingAsync(IPAddress ipAddress)
        {
            Interlocked.Increment(ref _WorkingTaskCount);
            try
            {
                byte[] buffer = { 1 };
                PingOptions options = new PingOptions(50, true);
                Ping ping = new Ping();
                ping.PingCompleted += new PingCompletedEventHandler(ping_Complete);
                PingReply reply = await ping.SendPingAsync(ipAddress, 5000, buffer, options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
            }
        }

        private void ping_Complete(object sender, PingCompletedEventArgs e)
        {
            Interlocked.Decrement(ref _WorkingTaskCount);
            try
            {
                if (e.Reply.Status == IPStatus.Success)
                {
                    IPInfo ipInfo = new IPInfo(e.Reply.Address, e.Reply.RoundtripTime);
                    ipInfo.HostDetailsBeforeChanged += TSub_BeforeChanged;
                    ipInfo.HostDetailsAfterChanged += TSub_AfterChanged;
                    ipInfo.setHostDetailsAsync();
                    buffer.AddLine(ipInfo);
                }
            }
            catch (Exception)
            {

            }
            Interlocked.Increment(ref _progress);
            Tasks.Remove(e.Reply.Address.ToString());
            //Interlocked.Increment(ref _currentPosition);
        }

        private async Task<PingReply> PingHostAsync(IPAddress Address, ManualResetEvent manualResetEvent)
        {
            //http://stackoverflow.com/questions/11800958/using-ping-in-c-sharp
            Ping pinger = new Ping();
            PingReply reply = null;
            try
            {
                reply = await pinger.SendPingAsync(Address, timeOut, new byte[] { 0 }, new PingOptions(64, true));
                manualResetEvent.Set();
            }
            catch (PingException ex)
            {
                // Discard PingExceptions and return false;       
                Debug.WriteLine(ex.StackTrace);
            }
            return reply;
        }

        public override void Stop()
        {
            foreach (IPInfo item in buffer.Buffer)
            {
                item.StopLooking4HostDetails(null);
            }
            wasStopped = true;
            isRunning = false;
        }

        /*
        public bool isRunning { get; private set; }        
        public bool wasStopped { get; private set; }
        public CancellationToken cancellationToken { get; private set; }
        public int taskId { get; private set; }
        private int _pauseTime;
        public int pauseTime
        {
            get { return _pauseTime; }
            set { _pauseTime = value >= 0 ? value : 0; }
        }
        private checkIPSearchTask checkTasks { get; set; }
        public BufferedResult<IPInfo> buffer { get; private set; }        
        public Dictionary<IPAddress, bool> isLooking4HostNames { get; private set; }
        public Dictionary<int, int> Progress { get; private set; }
        public List<IPAddress> ipList { get; set; }
        public int index { get; private set; }
        public int currentPosition { get; private set; }
        public int count { get; set; }
        public int remaind
        {
            get
            {
                return (index + count - 1 - currentPosition);
            }
        }
        public int progress { get; private set; }
        private int timeOut { get; set; }
        public bool isPaused { get; private set; }
        private Action<IPInfo> bufferResultAddLine { get; set; }

        private byte[] pingBuffer = Encoding.ASCII.GetBytes(".");
        private PingOptions options = new PingOptions(50, true);
        private AutoResetEvent reset = new AutoResetEvent(false);

        public IPSearchTask(int TaskId, List<IPAddress> IPList, int Index, int Count, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, checkIPSearchTask CheckTasks)
        {
            buffer = new BufferedResult<IPInfo>();
            isLooking4HostNames = new Dictionary<IPAddress, bool>();
            Progress = new Dictionary<int, int>();
            taskId = TaskId;
            pauseTime = 0;
            checkTasks = CheckTasks;
            ipList = IPList;
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

        private PingReply PingHost(IPAddress Address)
        {
            //http://stackoverflow.com/questions/11800958/using-ping-in-c-sharp
            Ping pinger = new Ping();
            PingReply reply = null;
            try
            {
                reply = pinger.Send(Address, timeOut, new byte[] { 0 }, new PingOptions(64, true));
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return reply;
        }
        
        void HostName_BeforeChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                isLooking4HostNames.Add(((IPInfo)sender).IPAddress, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }
        private void HostName_AfterChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                isLooking4HostNames[((IPInfo)sender).IPAddress] = false;
            }
            catch (Exception) { }
        }

        public void Init(int Index, int Count)
        {
            index = currentPosition = Index;
            count = Count;
        }

        private void Search()
        {
            Console.WriteLine(taskId + " is started");
            bool waiting4CheckTasks = false;
            int checkTasksLoopTimeMax = 60;
            int sleepTime = 100;
            Progress.Add(index, currentPosition);
            if (!wasStopped)
            {
                while (isRunning && currentPosition < index + count && currentPosition < ipList.Count)
                {
                    TimeSpan checkTasksLoopTime = DateTime.Now - checkTasks.lastTime;
                    if (checkTasksLoopTime.TotalSeconds > checkTasksLoopTimeMax)
                    {
                        if (!waiting4CheckTasks)
                        {
                            Console.WriteLine(taskId.ToString() + " is waiting for checkTasks iterration. CheckTasks loop time: " + checkTasksLoopTime.TotalSeconds.ToString());
                            waiting4CheckTasks = true;
                        }
                        sleepTime += (int)(checkTasksLoopTime.TotalSeconds - checkTasksLoopTimeMax);
                        if (sleepTime > 1000)
                        {
                            sleepTime = 1000;
                        }
                    }
                    else
                    {
                        if (waiting4CheckTasks)
                        {
                            waiting4CheckTasks = false;
                            Console.WriteLine(taskId.ToString() + " resumed its work");
                        }
                        sleepTime = 100;
                    }

                    if (isPaused || waiting4CheckTasks)
                    {
                        Thread.Sleep(sleepTime);
                    }
                    else
                    {                        

                        IPAddress address = ipList[currentPosition];
                        PingReply reply = PingHost(address);
                        IPInfo ipInfo = new IPInfo(address);

                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            ipInfo.RoundtripTime = reply.RoundtripTime;
                            ipInfo.PropertyBeforeChanged += HostName_BeforeChanged;
                            ipInfo.PropertyAfterChanged += HostName_AfterChanged;
                            ipInfo.setHostNameAsync();
                            buffer.AddLine(ipInfo);
                        }

                        progress++;
                        currentPosition++;
                        Progress[index] = currentPosition;

                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Stop();
                    }
                }
            }
            isRunning = false;
            Console.WriteLine(taskId + " is stopped");
        }

        public void Start()
        {
            isRunning = true;
            Search();
        }
        public void Stop()
        {            
            foreach (IPInfo item in buffer.Buffer)
            {
                item.StopLooking4HostNames(null);
            }            
            wasStopped = true;
            isRunning = false;
        }
        public void Pause()
        {
            isPaused = !isPaused;
        }
        public void Pause(bool IsPaused)
        {
            isPaused = IsPaused;
        }
        */
    }
}
