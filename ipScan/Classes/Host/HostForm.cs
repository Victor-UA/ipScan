using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ipScan.Classes.Host.Grid;
using ipScan.Classes.IP;

namespace ipScan.Classes.Host
{
    public partial class HostForm : Form
    {
        public IPInfo ipInfo { get; private set; }
        public HostForm(IPInfo IPInfo)
        {
            InitializeComponent();
            ipInfo = IPInfo;            
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
                            Classes.Grid.Fill.GridFill(grid_HostOpenPorts, ipInfo.HostPorts,
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

        private void btn_ScanHostPorts_Click(object sender, EventArgs e)
        {
            ipInfo.ScanHostPorts();
        }
    }
}
