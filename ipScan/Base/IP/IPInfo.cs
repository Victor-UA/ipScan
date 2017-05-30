using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using ipScan.Base.IP.Host;

namespace ipScan.Base.IP
{
    public class IPInfo : object
    {
        public string                           IPAddress { get; set; }
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
        public PhysicalAddress                  HostMac { get; set; }

        public Thread                           look4HostDetails { get; private set; }
        public bool                             isLooking4HostDetails { get; private set; }
        private CancellationTokenSource         CancelScanHostPorts { get; set; }
        private int                             waitingForResponses;
        private int                             currentHostPort;
        private int                             maxWaitingForResponses { get; set; } = 200; 
               
        public EventedList<PortInfo>            Ports { get; private set; }
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

        public IPInfo(string ipAddress, string hostName, long roundtripTime)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            RoundtripTime = roundtripTime;
            HostForm = null;            
        }
        public IPInfo(string ipAddress) : this(ipAddress, string.Empty, 0)
        {
            
        }
        public IPInfo(string ipAddress, long roundtripTime) : this(ipAddress, string.Empty, roundtripTime)
        {
            
        }

        public event EventHandler HostDetailsBeforeChanged;
        public event EventHandler HostDetailsAfterChanged;        

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
                        Socket socket;

                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.ReceiveTimeout = 100;                      
                        socket.BeginConnect(new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), currentHostPort), (IAsyncResult asyncResult) =>
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
                                                socketResult.ProtocolType,
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

        public void                             setHostDetailsAsync()
        {
            look4HostDetails = new Thread(() => {
                isLooking4HostDetails = true;
                HostDetailsBeforeChanged(IPAddress, new EventArgs());
                try
                {
                    HostName = Dns.GetHostEntry(IPAddress).HostName;
                }
                catch (Exception)
                {
                    //Debug.WriteLine(IPAddress.ToString() + ": " + ex.Message);
                    HostName = string.Empty;
                }                

                try
                {
                    HostMac = IPTools.GetMACFromNetworkComputer(IPAddress);
                }
                catch (Exception)
                {
                    //Debug.WriteLine(IPAddress.ToString() + ": " + ex.Message);
                    HostMac = null;
                }

                isLooking4HostDetails = false;
                HostDetailsAfterChanged(IPAddress, new EventArgs());
            });
            look4HostDetails.Start();            
        }

        public void                                 StopLooking4HostDetails(object obj)
        {                      
            if (look4HostDetails != null)
            {
                try
                {
                    look4HostDetails.Abort();
                    //look4HostDetails.Join();
                }
                catch (Exception)
                {
                }
            }
        }
/*        
        public event PropertyChangedEventHandler    HostDetailsBeforeChanged;
        protected void                              OnHostDetailsBeforeChanged(PropertyChangedEventArgs e)
        {
            HostDetailsBeforeChanged?.Invoke(IPAddress, e);
        }
        protected void                              OnHostDetailsBeforeChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnHostDetailsBeforeChanged(new PropertyChangedEventArgs(propertyName));
        }
*/
/*
        public event PropertyChangedEventHandler    HostDetailsAfterChanged;
        protected void                              OnHostDetailsAferChanged(PropertyChangedEventArgs e)
        {
            HostDetailsAfterChanged?.Invoke(IPAddress, e);
        }
        protected void                              OnHostDetailsAfterChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnHostDetailsAferChanged(new PropertyChangedEventArgs(propertyName));
        }
*/      
        public string                               toString()
        {
            string[] ip = IPAddress.ToString().Split('.');
            string result = string.Empty;
            for (int i = 0; i < ip.Length; i++)
            {
                result += ip[i].PadLeft(3, ' ') + (i < ip.Length - 1 ? "." : string.Empty);
            }
            return result + "\t" + RoundtripTime.ToString().PadLeft(4, ' ') + "\t" + HostName;
        }
        public override string                      ToString()
        {
            return IPAddress.ToString();
        }

    }
}
