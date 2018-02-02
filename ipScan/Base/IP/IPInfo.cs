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
        private uint                            _IPAddress;
        public uint                             IPAddress
        {
            get
            {
                return _IPAddress;
            }
            set
            {
                _IPAddress = value;
                IPAddressStr = IPTools.UInt322IPAddressStr(value);
            }
        }
        public string                           IPAddressStr { get; private set; }
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
                        SetHostFormCaption();
                    }
                }
                catch
                {
                }
            }
        }
        public PhysicalAddress                  HostMac { get; set; }

        public Thread                           _look4HostDetails { get; private set; }
        public bool                             _isLooking4HostDetails { get; private set; }
        private CancellationTokenSource         CancelScanHostPorts { get; set; }
        private int                             waitingForResponses;
        private int                             CurrentHostPort { get; set; }
        private int                             _maxWaitingForResponses { get; set; } = 200; 
               
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

        public IPInfo(uint ipAddress, string hostName, long roundtripTime)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            RoundtripTime = roundtripTime;
            HostForm = null;            
        }
        public IPInfo(uint ipAddress) : this(ipAddress, string.Empty, 0)
        {
            
        }
        public IPInfo(uint ipAddress, long roundtripTime) : this(ipAddress, string.Empty, roundtripTime)
        {
            
        }

        public event EventHandler HostDetailsBeforeChanged;
        public event EventHandler HostDetailsAfterChanged;        

        private void                            SetHostFormCaption()
        {
            if (HostForm.InvokeRequired)
            {
                HostForm.Invoke(new Action(SetHostFormCaption));
            }
            else
            {
                HostForm.Text = IPAddressStr + " " + HostName;
            }
        }
        public void                             ShowHostForm(System.Windows.Forms.IWin32Window owner = null)
        {
            if (HostForm == null || HostForm.IsDisposed)
            {
                HostForm = new HostForm(this);
                SetHostFormCaption();                
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
                    HostForm.setScanPortsProgress(CurrentHostPort, waitingForResponses, _maxWaitingForResponses);
                }
                while (ScanTCPPortsIsRunning)
                {
                    Thread.Sleep(1000);
                    HostForm.setScanPortsProgress(CurrentHostPort, waitingForResponses, _maxWaitingForResponses);
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
                    
                for (CurrentHostPort = 1; CurrentHostPort <= 65535 && !CancelScanHostPorts.Token.IsCancellationRequested; CurrentHostPort++)
                {
                    while (waitingForResponses >= _maxWaitingForResponses)
                    {
                        Thread.Sleep(0);
                    }
                    try
                    {
                        ScanPortTCP(CurrentHostPort);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + "r" + ex.StackTrace);
                    }
                    
                }
                while (waitingForResponses > 0)
                {
                    Thread.Sleep(100);
                }
                ScanTCPPortsIsRunning = false;
            });            
        }
        private void ScanPortTCP(int currentHostPort)
        {
            try
            {
                Socket socket;

                socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );
                socket.ReceiveTimeout = (int)(RoundtripTime * 1.1 + 1);
                socket.BeginConnect(new IPEndPoint(IPTools.UInt322IPAddress(IPAddress), currentHostPort),
                    (IAsyncResult asyncResult) =>
                    {
                        try
                        {                            
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
                        finally
                        {
                            Interlocked.Decrement(ref waitingForResponses);
                        }
                    },
                    socket
                );
                Interlocked.Increment(ref waitingForResponses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }            
        }
        private void ScanPortUDP(int currentHostPort)
        {
            try
            {
                UdpClient udpClient = new UdpClient(11000);
                Socket socket = udpClient.Client;
                socket.ExclusiveAddressUse = true;
                socket.ReceiveTimeout = (int)(RoundtripTime * 1.1 + 1);

                socket.ReceiveTimeout = (int)(RoundtripTime * 1.1 + 1);
                socket.BeginConnect(new IPEndPoint(IPTools.UInt322IPAddress(IPAddress), currentHostPort),
                    (IAsyncResult asyncResult) =>
                    {
                        try
                        {
                            /*
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
                            */
                        }
                        catch (Exception)
                        {                            
                        }
                        finally
                        {
                            Interlocked.Decrement(ref waitingForResponses);
                        }
                    },
                    socket
                );
                Interlocked.Increment(ref waitingForResponses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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

        public void                             SetHostDetailsAsync()
        {
            _look4HostDetails = new Thread(() => {
                _isLooking4HostDetails = true;
                HostDetailsBeforeChanged(IPAddress, new EventArgs());
                try
                {
                    HostName = Dns.GetHostEntry(IPTools.UInt322IPAddress(IPAddress)).HostName;
                }
                catch (Exception)
                {
                    //Debug.WriteLine(IPAddress.ToString() + ": " + ex.Message);
                    HostName = string.Empty;
                }                

                try
                {
                    HostMac = IPTools.GetMACFromNetworkComputer(IPTools.UInt322IPAddress(IPAddress));
                }
                catch (Exception)
                {
                    //Debug.WriteLine(IPAddress.ToString() + ": " + ex.Message);
                    HostMac = null;
                }

                _isLooking4HostDetails = false;
                HostDetailsAfterChanged(IPAddress, new EventArgs());
            });
            _look4HostDetails.Start();            
        }

        public void                             StopLooking4HostDetails(object obj)
        {                      
            if (_look4HostDetails != null)
            {
                try
                {
                    _look4HostDetails.Abort();
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
