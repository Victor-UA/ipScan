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
using System.Runtime.InteropServices;

namespace ipScan.Classes.Main
{
    class IPSearchTask : SearchTask<IPInfo, uint>
    {
        public IPSearchTask(int TaskId, uint firstIPAddress, uint Count, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ITasksChecking CheckTasks)
            : base(TaskId, firstIPAddress, Count, BufferResultAddLine, TimeOut, CancellationToken, CheckTasks) { }

        protected override void Search()
        {            
            Debug.WriteLine(taskId + " is started " + DateTime.Now + "." + DateTime.Now.Millisecond);            
            bool waiting4TasksChecking = false;            
            int sleepTime = 100;
            Progress.Add(FirstIPAddress, CurrentPosition);

            byte[] buffer = { 1 };
            PingOptions options = new PingOptions(254, true);                        

            if (!wasStopped)
            {
                while (isRunning && CurrentPosition < FirstIPAddress + Count)
                {
                    #region Is Task has to be paused

                    #region (DISABLED) Check memory
                    /*
                                ComputerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                                ulong freeMemory = (ulong)(ComputerInfo.AvailablePhysicalMemory * 0.9);
                                int deltaCount = (int)(freeMemory / 500000) - (maxTaskCount - WorkingTaskCount);
                                int newMaxTaskCount = maxTaskCount + deltaCount;
                                maxTaskCount = (newMaxTaskCount > 0) ? (newMaxTaskCount < maxTaskCountLimit) ? newMaxTaskCount : maxTaskCountLimit : maxTaskCount;
                                */
                    #endregion                    

                    TimeSpan tasksCheckingLoopTime = DateTime.Now - TasksChecking.LastTime;                    
                    if (tasksCheckingLoopTime.TotalMilliseconds > TasksChecking.SleepTime * 10)
                    {
                        #region Calculate sleepTime
                        if (!waiting4TasksChecking)
                        {
                            Debug.WriteLine(taskId.ToString() + " is waiting for checkTasks iterration. CheckTasks loop time: " + tasksCheckingLoopTime.TotalSeconds.ToString());
                            waiting4TasksChecking = true;
                        }

                        sleepTime += (int)(tasksCheckingLoopTime.TotalSeconds - TasksChecking.SleepTime);
                        if (sleepTime > 1000 || sleepTime < 0)
                        {
                            sleepTime = 1000;
                        }                        
                        #endregion
                    }
                    else
                    {
                        if (waiting4TasksChecking)
                        {
                            waiting4TasksChecking = false;
                            Debug.WriteLine(taskId.ToString() + " resumed its work");
                        }
                        sleepTime = 100;
                    }
                    #endregion

                    if (isPaused || waiting4TasksChecking)
                    {
                        Thread.Sleep(sleepTime);
                    }
                    else
                    {
                        PingHost(CurrentPosition, timeOut, buffer, options);                        
                        CurrentPosition++;
                        Progress[FirstIPAddress] = CurrentPosition;
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Stop();
                    }                    
                }
            }                      
            isRunning = false;
            Debug.WriteLine(taskId + " is stopped");
        }

        #region (DISABLED) PingHostAsync
        ////https://stackoverflow.com/questions/24158814/ping-sendasync-always-successful-even-if-client-is-not-pingable-in-cmd
        //private async Task PingHostAsync(uint ipAddress, int TimeOut, byte[] buffer, PingOptions options)
        //{
        //    Interlocked.Increment(ref _WorkingTaskCount);
        //    try
        //    {
        //        Ping ping = new Ping();
        //        PingReply reply = await ping.SendPingAsync(IPTools.UInt322IPAddressStr(ipAddress), TimeOut, buffer, options);
        //        if (!wasStopped) {
        //            try
        //            {                        
        //                if (reply.Status == IPStatus.Success)
        //                {
        //                    uint address = IPTools.IPAddress2UInt32(reply.Address);                            
        //                    IPInfo ipInfo = new IPInfo(address, reply.RoundtripTime);
        //                    ipInfo.HostDetailsBeforeChanged += TSub_BeforeChanged;
        //                    ipInfo.HostDetailsAfterChanged += TSub_AfterChanged;
        //                    ipInfo.setHostDetailsAsync();
        //                    Buffer.AddLine(ipInfo);
        //                }                        
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
        //            }
        //        }
        //        //Interlocked.Decrement(ref _WorkingTaskCount);
        //        lock (Locker)
        //        {
        //            _WorkingTaskCount--;
        //            _progress++;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
        //    }

        //    if (!wasStopped)
        //    {
        //        try
        //        {
        //            lock (Locker)
        //            {
        //                Tasks.Remove(ipAddress);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
        //        }
        //    }
        //} 
        #endregion
        private void PingHost(uint ipAddress, int TimeOut, byte[] buffer, PingOptions options)
        {            
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(IPTools.UInt322IPAddressStr(ipAddress), TimeOut, buffer, options);                
                if (!wasStopped)
                {
                    try
                    {
                        if (reply.Status == IPStatus.Success)
                        {
                            uint address = IPTools.IPAddress2UInt32(reply.Address);
                            IPInfo ipInfo = new IPInfo(address, reply.RoundtripTime);
                            ipInfo.HostDetailsBeforeChanged += TSub_BeforeChanged;
                            ipInfo.HostDetailsAfterChanged += TSub_AfterChanged;
                            ipInfo.setHostDetailsAsync();
                            Buffer.AddLine(ipInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
                    }
                }
                Interlocked.Increment(ref _progress);                
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
                wasStopped = true;
                isRunning = false;
            }
        }        
    }
}
