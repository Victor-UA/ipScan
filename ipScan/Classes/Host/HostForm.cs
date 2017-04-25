using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            this.Text = IPInfo.IPAddress.ToString();
        }

    }
}
