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
using NLog;

namespace ipScan.Classes.Main
{
    class IPSearchTask : SearchTask<IPInfo, uint>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public IPSearchTask(int TaskId, uint firstIPAddress, uint Count, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ITasksChecking CheckTasks)
            : base(TaskId, firstIPAddress, Count, BufferResultAddLine, TimeOut, CancellationToken, CheckTasks) { }

        protected override void Search()
        {
            _logger.Trace(string.Format("Task [{0}] started", TaskId));            
            bool waiting4TasksChecking = false;            
            int sleepTime = 100;
            ProgressDict.TryAdd(FirstIPAddress, CurrentPosition);

            byte[] buffer = { 1 };
            PingOptions options = new PingOptions(254, true);                        

            if (!WasStopped)
            {
                while (IsRunning && CurrentPosition < FirstIPAddress + Count)
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
                            _logger.Trace(string.Format("Task [{0}] is waiting for checkTasks iterration. CheckTasks loop time: {1}",
                                TaskId, tasksCheckingLoopTime.TotalSeconds));
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
                            _logger.Trace(string.Format("Task [{0}] resumed its work", TaskId));
                        }
                        sleepTime = 100;
                    }
                    #endregion

                    if (waiting4TasksChecking)
                    {
                        Thread.Sleep(sleepTime);
                    }
                    else if (IsPaused)
                    {
                        Thread.Sleep(PAUSE_SLEEP_TIME);
                    }
                    else
                    {
                        PingHost(CurrentPosition, _timeOut, buffer, options);
                        CurrentPosition++;
                        ProgressDict[FirstIPAddress] = CurrentPosition;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Stop();
                        _logger.Trace(string.Format("Task [{0}] cancelled", TaskId));
                    }                    
                }
            }                      
            IsRunning = false;
            _logger.Trace(string.Format("Task [{0}] stopped", TaskId));            
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
                if (!WasStopped)
                {
                    try
                    {
                        if (reply.Status == IPStatus.Success)
                        {
                            uint address = IPTools.IPAddress2UInt32(reply.Address);
                            IPInfo ipInfo = new IPInfo(address, reply.RoundtripTime);
                            ipInfo.HostDetailsBeforeChanged += TSub_BeforeChanged;
                            ipInfo.HostDetailsAfterChanged += TSub_AfterChanged;
                            ipInfo.SetHostDetailsAsync();
                            Buffer.AddLine(ipInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
                Interlocked.Increment(ref _progress);                
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public override void Stop()
        {
            lock (_locker)
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
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                WasStopped = true;
                IsRunning = false;
            }
        }        
    }
}
