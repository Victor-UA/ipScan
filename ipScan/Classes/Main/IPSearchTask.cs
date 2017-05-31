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
using System.Runtime.InteropServices;

namespace ipScan.Classes.Main
{
    class IPSearchTask : SearchTask<IPInfo, uint>
    {        

        public IPSearchTask(int TaskId, List<uint> IPList, int Index, int Count, int maxTaskCountLimit, Action<IPInfo> BufferResultAddLine, int TimeOut, CancellationToken CancellationToken, ICheckSearchTask CheckTasks)
            : base(TaskId, IPList, Index, Count, BufferResultAddLine, TimeOut, maxTaskCountLimit, CancellationToken, CheckTasks) { }        
        
        protected override void Search()
        {            
            Debug.WriteLine(taskId + " is started " + DateTime.Now + "." + DateTime.Now.Millisecond);            
            bool waiting4CheckTasks = false;
            int checkTasksLoopTimeMax = 1000; //мілісекунди
            int sleepTime = 100;
            Progress.Add(index, currentPosition);

            byte[] buffer = { 1 };
            PingOptions options = new PingOptions(50, true);            

            if (!wasStopped)
            {
                Tasks = new Dictionary<object, Task>();
                WorkingTaskCount = 0;
                

                while (isRunning && currentPosition < index + count && currentPosition < mainList.Count)
                {

                    /*
                    ComputerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                    ulong freeMemory = (ulong)(ComputerInfo.AvailablePhysicalMemory * 0.9);
                    int deltaCount = (int)(freeMemory / 500000) - (maxTaskCount - WorkingTaskCount);
                    int newMaxTaskCount = maxTaskCount + deltaCount;
                    maxTaskCount = (newMaxTaskCount > 0) ? (newMaxTaskCount < maxTaskCountLimit) ? newMaxTaskCount : maxTaskCountLimit : maxTaskCount;
                    */

                    int maxTaskCount = MaxTaskCountLimit;

                    TimeSpan checkTasksLoopTime = DateTime.Now - checkTasks.LastTime;                    
                    
                    if (checkTasksLoopTime.TotalMilliseconds > checkTasksLoopTimeMax * 10)
                    {
                        if (!waiting4CheckTasks)
                        {
                            Debug.WriteLine(taskId.ToString() + " is waiting for checkTasks iterration. CheckTasks loop time: " + checkTasksLoopTime.TotalSeconds.ToString());
                            waiting4CheckTasks = true;
                        }
                        
                        sleepTime += (int)(checkTasksLoopTime.TotalSeconds - checkTasksLoopTimeMax);
                        if (sleepTime > 1000 || sleepTime < 0)
                        {
                            sleepTime = 1000;
                        }

                        //Debug.WriteLine("------------------------" + (int)(maxTaskCount * 0.95));
                        //int newMaxTaskCount = (int)(maxTaskCount * 0.9);
                        //maxTaskCount = newMaxTaskCount < 1 ? 1 : newMaxTaskCount;
                        maxTaskCount = MaxTaskCountLimit / 2;


                    }
                    else
                    {
                        if (waiting4CheckTasks)
                        {
                            waiting4CheckTasks = false;
                            Debug.WriteLine(taskId.ToString() + " resumed its work");
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
                            
                            //if (checkTasksLoopTime.TotalMilliseconds <= checkTasksLoopTimeMax && checkTasks.MySearchTasksStartedAll)
                            if (checkTasksLoopTime.TotalMilliseconds < checkTasksLoopTimeMax * 1.1
                                && 
                                checkTasks.loopTime.TotalMilliseconds < checkTasksLoopTimeMax * 1.1)
                                {
                                //Debug.WriteLine("++++++++++++++++++++++++" + (int)(maxTaskCount * 1.2));
                                //int newMaxTaskCount = (int)(maxTaskCount * 1.1);
                                //maxTaskCount = newMaxTaskCount > maxTaskCount ? 
                                //    newMaxTaskCount <= maxTaskCountLimit ? 
                                //    newMaxTaskCount : maxTaskCountLimit : ++maxTaskCount;
                                maxTaskCount = MaxTaskCountLimit;
                            }                         
                                                           
                            Thread.Sleep(100);
                        }
                        else
                        {
                            uint address = mainList[currentPosition];
                            lock(Locker)
                            {
                                Tasks.Add(address, PingHostAsync(address, timeOut, buffer, options));
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
            
            while (isRunning && WorkingTaskCount > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Stop();
                }
                Thread.Sleep(1000);
            }
            isRunning = false;
            Debug.WriteLine(taskId + " is stopped");
        }

        //https://stackoverflow.com/questions/24158814/ping-sendasync-always-successful-even-if-client-is-not-pingable-in-cmd
        private async Task PingHostAsync(uint ipAddress, int TimeOut, byte[] buffer, PingOptions options)
        {
            Interlocked.Increment(ref _WorkingTaskCount);
            try
            {
                Ping ping = new Ping();

                PingReply reply = await ping.SendPingAsync(IPTools.UInt322IPAddressStr(ipAddress), TimeOut, buffer, options);

                if (!wasStopped) {
                    try
                    {
                        if (reply.Status == IPStatus.Success)
                        {
                            uint address = IPTools.IPAddress2UInt32(reply.Address);
                            /*
                            foreach (IPAddress item in mainList)
                            {
                                if (item.ToString() == reply.Address.ToString())
                                {
                                    address = item;
                                    break;
                                }
                            }
                            */
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
                Interlocked.Decrement(ref _WorkingTaskCount);
                Interlocked.Increment(ref _progress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
            }

            if (!wasStopped)
            {
                try
                {
                    lock (Locker)
                    {
                        Tasks.Remove(ipAddress);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "\r" + ex.StackTrace);
                }
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
                    try
                    {
                        Tasks.Clear();
                        Tasks = null;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    break;
                }
                wasStopped = true;
                isRunning = false;
            }
        }        
    }
}
