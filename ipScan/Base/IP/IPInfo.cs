﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ipScan.Classes.Host;

namespace ipScan.Base.IP
{
    public class IPInfo : object
    {
        public IPAddress                        IPAddress { get; set; }
        public long                             RoundtripTime { get; set; }

        public HostForm                         HostForm { get; private set; }
        private string _HostName;
        public string                           HostName
        {
            get { return _HostName; }
            set
            {
                _HostName = value;

                try
                {
                    if (HostForm != null)
                    {
                        setHostFormCaption();
                    }
                }
                catch
                {
                }
            }
        }

        public Thread                           look4HostName { get; private set; }
        public bool                             isLooking4HostName { get; private set; }
        private CancellationTokenSource         CancelScanHostPorts { get; set; }
        private int                             waitingForResponses;
        private int                             currentHostPort;
        private int                             maxWaitingForResponses { get; set; } = 200; 
               
        public EventedList<PortInfo>              Ports { get; private set; }
        private bool                            _ScanPortsIsRunning;
        public bool                             ScanTCPPortsIsRunning
        {
            get { return _ScanPortsIsRunning; }
            private set
            {
                _ScanPortsIsRunning = value;
                HostForm.switch_btn_ScanHostPorts(value);
            }
        }

        public IPInfo(IPAddress ipAddress, string hostName, long roundtripTime)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            RoundtripTime = roundtripTime;
            HostForm = null;            
        }
        public IPInfo(IPAddress ipAddress) : this(ipAddress, string.Empty, 0)
        {
            
        }
        public IPInfo(IPAddress ipAddress, long roundtripTime) : this(ipAddress, string.Empty, roundtripTime)
        {
            
        }

        private void                            setHostFormCaption()
        {
            if (HostForm.InvokeRequired)
            {
                HostForm.Invoke(new Action(setHostFormCaption));
            }
            else
            {
                HostForm.Text = IPAddress.ToString() + " " + HostName;
            }
        }
        public void                             ShowHostForm(System.Windows.Forms.IWin32Window owner = null)
        {
            if (HostForm == null || HostForm.IsDisposed)
            {
                HostForm = new HostForm(this);
                setHostFormCaption();                
            }
            if (HostForm.Visible)
            {
                if (HostForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                {
                    HostForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                }
                HostForm.Focus();
            }
            else
            {                
                HostForm.Show(owner);
            }
        }

        public void                             ScanHostPorts()
        {
            CancelScanHostPorts = new CancellationTokenSource();
            Task checkScanHostPorts = Task.Run(() =>
            {
                while (!ScanTCPPortsIsRunning)
                {
                    Thread.Sleep(1000);
                    HostForm.setScanPortsProgress(currentHostPort, waitingForResponses, maxWaitingForResponses);
                }
                while (ScanTCPPortsIsRunning)
                {
                    Thread.Sleep(1000);
                    HostForm.setScanPortsProgress(currentHostPort, waitingForResponses, maxWaitingForResponses);
                }
            });
            //https://www.codeproject.com/Articles/199016/Simple-TCP-Port-Scanner
            Task ScanHostTCPPorts = Task.Run(() =>
            {
                ScanTCPPortsIsRunning = true;
                Ports = new EventedList<PortInfo>();
                Ports.onChanged += () => 
                {
                    HostForm.FillHostOpenPorts();
                };
                waitingForResponses = 0;
                    
                for (currentHostPort = 0; currentHostPort < 65535 && !CancelScanHostPorts.Token.IsCancellationRequested; currentHostPort++)
                {
                    while (waitingForResponses >= maxWaitingForResponses)
                    {
                        Thread.Sleep(0);
                    }
                    try
                    {
                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);                        
                        socket.BeginConnect(new IPEndPoint(IPAddress, currentHostPort), (IAsyncResult asyncResult) =>
                        {
                            try
                            {
                                Interlocked.Decrement(ref waitingForResponses);
                                Socket socketResult = asyncResult.AsyncState as Socket;
                                socketResult.EndConnect(asyncResult);
                                if (socketResult.Connected)
                                {
                                    try
                                    {
                                        Ports.Add(new PortInfo
                                            (
                                                int.Parse(socketResult.RemoteEndPoint.ToString().Split(':')[1]),
                                                ProtocolType.Tcp,
                                                true
                                            )
                                        );
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.StackTrace);
                                    }
                                }                                
                            }
                            catch
                            {
                            }
                        },
                            socket
                        );
                        Interlocked.Increment(ref waitingForResponses);
                    }
                    catch
                    {
                    }
                    
                }
                while (waitingForResponses > 0)
                {
                    Thread.Sleep(100);
                }
                ScanTCPPortsIsRunning = false;
            });            
        }
        public void                             StopScanHostPorts()
        {
            try
            {
                CancelScanHostPorts.Cancel();
            }
            catch (Exception)
            {
            }
        }

        public void                             setHostNameAsync()
        {
            look4HostName = new Thread(() => {
                isLooking4HostName = true;
                OnPropertyBeforeChanged("HostName");
                try
                {
                    //Timer timer = new Timer(new TimerCallback(StopLooking4HostNames), null, 1000, Timeout.Infinite);
                    HostName = Dns.GetHostEntry(IPAddress).HostName;
                    //HostName = (await Dns.GetHostEntryAsync(IPAddress)).HostName;
                    //Debug.WriteLine(HostName);
                    //timer.Dispose();
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(IPAddress.ToString() + ": " + ex.Message);
                    HostName = string.Empty;
                }
                finally
                {
                    isLooking4HostName = false;
                    OnPropertyAfterChanged("HostName");
                }
            });
            look4HostName.Start();
            /*
            isLooking4HostNames = true;
            OnPropertyBeforeChanged("HostName");
            try
            {
                HostName = (await Dns.GetHostEntryAsync(IPAddress)).HostName;                
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(IPAddress.ToString() + ": " + ex.Message);   
                HostName = string.Empty;
            }
            finally
            {
                isLooking4HostNames = false;
                OnPropertyAfterChanged("HostName");
            }
            */
        }
        public void                             StopLooking4HostNames(object obj)
        {
            /*
            isLooking4HostNames = false;
            OnPropertyAfterChanged("HostName");   
            */
            
            if (look4HostName != null)
            {
                try
                {
                    look4HostName.Abort();
                    look4HostName.Join();
                }
                catch (Exception)
                {
                }
            }
        }
        
        public event PropertyChangedEventHandler    PropertyBeforeChanged;
        protected void                              OnPropertyBeforeChanged(PropertyChangedEventArgs e)
        {
            PropertyBeforeChanged?.Invoke(IPAddress, e);
        }
        protected void                              OnPropertyBeforeChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyBeforeChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler    PropertyAfterChanged;
        protected void                              OnPropertyAferChanged(PropertyChangedEventArgs e)
        {
            PropertyAfterChanged?.Invoke(IPAddress, e);
        }
        protected void                              OnPropertyAfterChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyAferChanged(new PropertyChangedEventArgs(propertyName));
        }
        
        public string                           toString()
        {
            string[] ip = IPAddress.ToString().Split('.');
            string result = string.Empty;
            for (int i = 0; i < ip.Length; i++)
            {
                result += ip[i].PadLeft(3, ' ') + (i < ip.Length - 1 ? "." : string.Empty);
            }
            return result + "\t" + RoundtripTime.ToString().PadLeft(4, ' ') + "\t" + HostName;
        }
        public override string                  ToString()
        {
            return IPAddress.ToString();
        }
    }
}
