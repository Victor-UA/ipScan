﻿using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ipScan.Classes
{
    class IPInfo 
    {
        public IPAddress IPAddress { get; set; }
        public string HostName { get; set; }
        public long RoundtripTime { get; set; }
        public bool isLooking4HostNames { get; private set; }
        public IPInfo(IPAddress ipAddress, string hostName, long roundtripTime)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            RoundtripTime = roundtripTime;
        }
        public IPInfo(IPAddress ipAddress) : this(ipAddress, string.Empty, 0)
        {
            
        }
        public IPInfo(IPAddress ipAddress, long roundtripTime) : this(ipAddress, string.Empty, roundtripTime)
        {
            
        }

        public void getHostName()
        {
            isLooking4HostNames = true;
            OnPropertyBeforeChanged("HostName");
            try
            {
                HostName = (Dns.GetHostEntry(IPAddress)).HostName;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled");
            }
            catch (Exception)
            {
                HostName = string.Empty;                
            }
            finally
            {
                isLooking4HostNames = false;
                OnPropertyAfterChanged("HostName");
            }
        }

        public async void setHostNameAsync()
        {
            //Task.Factory.StartNew(getHostName);
            
            isLooking4HostNames = true;
            OnPropertyBeforeChanged("HostName");

            try
            {
                Timer timer = new Timer(new TimerCallback(StopLooking4HostNames), null, 10, Timeout.Infinite);
                HostName = (await Dns.GetHostEntryAsync(IPAddress)).HostName;
                timer.Dispose();
            }
            catch (Exception)
            {
                HostName = string.Empty;
            }
            finally
            {
                isLooking4HostNames = false;
                OnPropertyAfterChanged("HostName");
            }
            
        }
        public void StopLooking4HostNames(object obj)
        {
            isLooking4HostNames = false;
            OnPropertyAfterChanged("HostName");
        }

        
        public event PropertyChangedEventHandler PropertyBeforeChanged;
        protected void OnPropertyBeforeChanged(PropertyChangedEventArgs e)
        {
            PropertyBeforeChanged?.Invoke(this, e);
        }
        protected void OnPropertyBeforeChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyBeforeChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyAfterChanged;
        protected void OnPropertyAferChanged(PropertyChangedEventArgs e)
        {
            PropertyAfterChanged?.Invoke(this, e);
        }
        protected void OnPropertyAfterChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyAferChanged(new PropertyChangedEventArgs(propertyName));
        }
        
        public string toString()
        {
            string[] ip = IPAddress.ToString().Split('.');
            string result = string.Empty;
            for (int i = 0; i < ip.Length; i++)
            {
                result += ip[i].PadLeft(3, ' ') + (i < ip.Length - 1 ? "." : string.Empty);
            }
            return result + "\t" + RoundtripTime.ToString().PadLeft(4, ' ') + "\t" + HostName;
        }
    }
}
