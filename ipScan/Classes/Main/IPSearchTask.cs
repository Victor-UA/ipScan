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

        const int MAX_SLEEP_TIME = 10000;

        public IPSearchTask(int TaskId, uint firstIPAddress, uint Count, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ITasksChecking CheckTasks)
            : base(TaskId, firstIPAddress, Count, BufferResultAddLine, TimeOut, CancellationToken, CheckTasks) { }

        protected override void Search()
        {
            _logger.Trace(string.Format("Task [{0}] started", TaskId));            
            
            int sleepTime = 100;
            ProgressDict.TryAdd(FirstIPAddress, CurrentPosition);

            byte[] buffer = { 1 };
            PingOptions options = new PingOptions(254, true);            

            if (!WasStopped)
            {
                while (IsRunning)
                {
                    if (Remaind > 0)
                    {
                        #region (DISABLED) Is Task has to be paused

                        #region (DISABLED) Check memory
                        /*
                                    ComputerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                                    ulong freeMemory = (ulong)(ComputerInfo.AvailablePhysicalMemory * 0.9);
                                    int deltaCount = (int)(freeMemory / 500000) - (maxTaskCount - WorkingTaskCount);
                                    int newMaxTaskCount = maxTaskCount + deltaCount;
                                    maxTaskCount = (newMaxTaskCount > 0) ? (newMaxTaskCount < maxTaskCountLimit) ? newMaxTaskCount : maxTaskCountLimit : maxTaskCount;
                                    */
                        #endregion

                        //TimeSpan tasksCheckingLoopTime = DateTime.Now - CheckTasks.LastTime;
                        //if (tasksCheckingLoopTime.TotalMilliseconds > CheckTasks.SleepTime * 2)
                        //{
                        //    #region Calculate sleepTime
                        //    sleepTime += (int)(tasksCheckingLoopTime.TotalMilliseconds - CheckTasks.SleepTime);
                        //    if (sleepTime > MAX_SLEEP_TIME || sleepTime < 0)
                        //    {
                        //        sleepTime = MAX_SLEEP_TIME;
                        //    }
                        //    if (!Waiting4TasksChecking)
                        //    {
                        //        _logger.Trace($"Task [{TaskId}] is waiting for checkTasks iterration {sleepTime} ms. CheckTasks loop time: {tasksCheckingLoopTime.TotalSeconds}");
                        //        Waiting4TasksChecking = true;
                        //    }
                        //    #endregion
                        //}
                        //else
                        //{
                        //    Waiting4TasksChecking = false;
                        //    sleepTime = PAUSE_SLEEP_TIME;
                        //}
                        #endregion

                        if (Waiting4TasksChecking)
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
                    else
                    {
                        IsRunning = CheckTasks.ChangeSearchTaskRange(TaskId);
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
                string ip = IPTools.UInt322IPAddressStr(ipAddress);
                PingReply reply = ping.Send(ip, TimeOut, buffer, options);                
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
                _logger.Trace(string.Concat(TaskId, ": ", ip, ": ", reply.Status));
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
