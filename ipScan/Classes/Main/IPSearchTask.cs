using System;
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
            maxTaskCount = 99;
            bool waiting4CheckTasks = false;
            int checkTasksLoopTimeMax = 2; //секунди
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
                        
                        Debug.WriteLine("------------------------" + (int)(maxTaskCount * 0.95));
                        maxTaskCount = (int)(maxTaskCount * 0.95) < maxTaskCount ? (int)(maxTaskCount * 0.95) : 1;
                        
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
                        if (WorkingTaskCount >= maxTaskCount)
                        {
                            
                            if (checkTasksLoopTime.TotalSeconds <= checkTasksLoopTimeMax)
                            {
                                Debug.WriteLine("++++++++++++++++++++++++" + (int)(maxTaskCount * 1.2));
                                maxTaskCount = (int)(maxTaskCount * 1.2) > maxTaskCount ? (int)(maxTaskCount * 1.2) : ++maxTaskCount;
                            }
                            
                            Thread.Sleep(100);
                        }
                        else
                        {
                            IPAddress address = mainList[currentPosition];
                            Task task = PingHostAsync(address);
                            lock(Locker)
                            {
                                Tasks.Add(address.ToString(), task);
                            }
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
        private async Task PingHostAsync(IPAddress ipAddress)
        {
            Interlocked.Increment(ref _WorkingTaskCount);
            try
            {
                byte[] buffer = { 1 };
                PingOptions options = new PingOptions(50, true);
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ipAddress, 5000, buffer, options);
                
                try
                {
                    if (reply.Status == IPStatus.Success)
                    {
                        IPInfo ipInfo = new IPInfo(reply.Address, reply.RoundtripTime);
                        ipInfo.HostDetailsBeforeChanged += TSub_BeforeChanged;
                        ipInfo.HostDetailsAfterChanged += TSub_AfterChanged;
                        ipInfo.setHostDetailsAsync();
                        Buffer.AddLine(ipInfo);
                    }
                }
                catch (Exception)
                {

                }
                Interlocked.Decrement(ref _WorkingTaskCount);
                Interlocked.Increment(ref _progress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
            }
            
            try
            {
                lock (Locker)
                {
                    Tasks.Remove(ipAddress.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
            }            
        }          

        public override void Stop()
        {
            lock (Locker)
            {
                while (true)
                {
                    try
                    {
                        foreach (IPInfo item in Buffer.Buffer)
                        {
                            if (item != null)
                            {
                                item.StopLooking4HostDetails(null);
                            }
                        }

                    }
                    catch (Exception) { }
                    break;
                }
                wasStopped = true;
                isRunning = false;
            }
        }        
    }
}
