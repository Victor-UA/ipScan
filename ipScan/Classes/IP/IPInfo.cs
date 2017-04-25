using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using ipScan.Classes.Host;

namespace ipScan.Classes.IP
{
    public class IPInfo 
    {
        public IPAddress IPAddress { get; set; }
        public int Index { get; set; }
        public string HostName { get; set; }
        public long RoundtripTime { get; set; }
        public bool isLooking4HostNames { get; private set; }
        public Thread look4HostNames { get; private set; }
        public HostForm hostForm { get; private set; }

        public IPInfo(IPAddress ipAddress, string hostName, long roundtripTime, int Index = -1)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            RoundtripTime = roundtripTime;
            this.Index = Index;
            hostForm = null;
        }
        public IPInfo(IPAddress ipAddress, int Index = -1) : this(ipAddress, string.Empty, 0, Index)
        {
            
        }
        public IPInfo(IPAddress ipAddress, long roundtripTime) : this(ipAddress, string.Empty, roundtripTime)
        {
            
        }
        

        public void ShowHostForm(System.Windows.Forms.IWin32Window owner = null)
        {
            if (hostForm == null)
            {
                hostForm = new HostForm(this);
            }
            if (hostForm.Visible)
            {
                if (hostForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                {
                    hostForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                }
                hostForm.Focus();
            }
            else
            {
                hostForm.Show(owner);
            }
        }

        public void setHostNameAsync()
        {
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
            
            look4HostNames = new Thread(() => {
                isLooking4HostNames = true;
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
                    isLooking4HostNames = false;
                    OnPropertyAfterChanged("HostName");
                }
            });
            look4HostNames.Start();
            
        }
        public void StopLooking4HostNames(object obj)
        {
            /*
            isLooking4HostNames = false;
            OnPropertyAfterChanged("HostName");   
            */
            
            if (look4HostNames != null)
            {
                try
                {
                    look4HostNames.Abort();
                    look4HostNames.Join();
                }
                catch (Exception)
                {
                }
            }
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
        public override string ToString()
        {
            return IPAddress.ToString();
        }
    }
}
