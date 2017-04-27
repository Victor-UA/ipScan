using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using ipScan.Base;
using ipScan.Base.IP;

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
                            ipInfo.PropertyBeforeChanged += TSub_BeforeChanged;
                            ipInfo.PropertyAfterChanged += TSub_AfterChanged;
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

        public override void Stop()
        {
            foreach (IPInfo item in buffer.Buffer)
            {
                item.StopLooking4HostNames(null);
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
