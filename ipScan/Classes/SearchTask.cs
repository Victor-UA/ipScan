﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ipScan.Classes
{
    class SearchTask
    {        
        public bool isRunning { get; private set; }        
        public bool wasStopped { get; private set; }
        public CancellationToken cancellationToken { get; private set; }
        public int taskId { get; private set; }
        public BufferResult buffer { get; private set; }
        public BufferResult IpArePassed { get; private set; }
        public Dictionary<IPAddress, bool> isLooking4HostNames { get; private set; }
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

        public SearchTask(int TaskId, List<IPAddress> IPList, int Index, int Count, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken)
        {
            buffer = new BufferResult();
            IpArePassed = new BufferResult();
            isLooking4HostNames = new Dictionary<IPAddress, bool>();
            taskId = TaskId;
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

        private void isLooking4HostNamesTimeOuted() { }
        private void isLooking4HostNamesTimeOuted(IPAddress ipAddress, double timeLength)
        {
            DateTime timeStart = DateTime.Now;
            do
            {
                Thread.Sleep(100);
            } while ((DateTime.Now - timeStart).TotalMilliseconds < timeLength && isLooking4HostNames[ipAddress]);
            isLooking4HostNames[ipAddress] = false;
        }
        void HostName_BeforeChanged(object sender, PropertyChangedEventArgs e)
        {
            isLooking4HostNames.Add(((IPInfo)sender).IPAddress, true);
            Task.Factory.StartNew(isLooking4HostNamesTimeOuted);
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
        private void LookingForIp()
        {
            Console.WriteLine(taskId + " is started");
            if (!wasStopped)
            {
                while (isRunning && currentPosition < index + count && currentPosition < ipList.Count)
                {                    
                    //if (isPaused)
                    if (false)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        IPAddress address = ipList[currentPosition];
                        PingReply reply = PingHost(address);
                        IPInfo ipInfo = new IPInfo(address);
                        IpArePassed.AddLine(ipInfo);
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
            LookingForIp();
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
    }
}