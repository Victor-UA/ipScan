using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ipScan.Base.IP.Host
{
    public partial class HostForm : Form
    {
        public IPInfo ipInfo { get; private set; }
        private Grid.Fill fill;

        public HostForm(IPInfo IPInfo)
        {
            InitializeComponent();
            ipInfo = IPInfo;
            fill = new Grid.Fill();

            fill.GridFill(SG_HostOpenPorts, ipInfo.Ports, 
                (PortInfo item, Color color) =>
                {
                    return new Grid.GridCellController(Color.LightBlue);
                }, 
                new List<string>() { "Ports", "Protocol", "isOpen" });
            
            //Fill.GridFill(SG_HostOpenPorts, ipInfo.HostPorts, new GridCellController(ipInfo, Color.LightBlue), new List<string>() { "Ports", "Protocol" });

        }        

        public void FillHostOpenPorts()
        {
            try
            {                
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(FillHostOpenPorts));
                    }
                    else
                    {
                        try
                        {
                            fill.GridUpdateOrInsertRows(SG_HostOpenPorts, ipInfo.Ports,
                                (PortInfo item, Color color) =>
                                    {
                                        return new Grid.GridCellController(Color.LightBlue);
                                    }
                            );                            
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            
        }

        public void switch_btn_ScanHostPorts(bool isRunning)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(switch_btn_ScanHostPorts), new object[] { isRunning });
            }
            else
            {
                btn_ScanHostPorts.Text = isRunning ? "Stop" : "Scan Ports";
            }
        }

        public void setScanPortsProgress(int index, int waitingForResponses, int maxWaitingForResponses)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action<int, int, int>(setScanPortsProgress), new object[] { index, waitingForResponses, maxWaitingForResponses });
                }
                catch
                {
                }
            }
            else
            {
                label_ScanPortsProgress.Text = index.ToString() + @" \ 65535 ( " + waitingForResponses.ToString() + @" \ " + maxWaitingForResponses.ToString() + " )";
            }
        }

        private void btn_ScanHostPorts_Click(object sender, EventArgs e)
        {
            if (ipInfo.ScanTCPPortsIsRunning)
            {
                ipInfo.StopScanHostPorts();                
            }
            else
            {
                fill.GridFill<int>(SG_HostOpenPorts, null, null, new List<string>() { "Ports", "Protocol", "isOpen" });
                ipInfo.ScanHostPorts();
            }
        }

        private void HostForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (ipInfo.ScanTCPPortsIsRunning)
                {
                    ipInfo.StopScanHostPorts();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                e.Cancel = false;
            }
        }        
    }
}
