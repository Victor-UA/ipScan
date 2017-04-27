using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ipScan.Base.Grid;
using ipScan.Base.IP;
using ipScan.Classes.Host.Grid;

namespace ipScan.Classes.Host
{
    public partial class HostForm : Form
    {
        public IPInfo ipInfo { get; private set; }
        

        public HostForm(IPInfo IPInfo)
        {
            InitializeComponent();
            ipInfo = IPInfo;
            
            Fill.GridFill(SG_HostOpenPorts, ipInfo.HostPorts, 
                (object item, Color color) =>
                {
                    return new GridCellController(item, Color.LightBlue);
                }, 
                new List<string>() { "Ports", "Protocol" });
            
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
                            Fill.GridUpdateOrInsertRows(SG_HostOpenPorts, ipInfo.HostPorts,
                                (object item, Color color) =>
                                    {
                                        return new GridCellController(item, Color.LightBlue);
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
                Fill.GridFill<int>(SG_HostOpenPorts, null, null, new List<string>() { "Ports", "Protocol" });
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
