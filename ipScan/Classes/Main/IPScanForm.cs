using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ipScan.Base;
using ipScan.Base.IP;
using System.Collections;
using System.Net.NetworkInformation;
using System.Text;

namespace ipScan.Classes.Main
{
    public partial class IPScanForm : Form
    {
        private Base.IP.Grid.Fill fill;
        private List<Task<PingReply>> myTasks { get; set; }
        private List<ISearchTask<IPInfo, IPAddress>> mySearchTasks { get; set; }
        private CancellationTokenSource mySearchTasksCancel { get; set; }
        private CheckSearchTask<IPInfo, IPAddress> checkTasks { get; set; }
        private bool _resultIsUpdatable = true;
        private bool resultIsUpdatable
        {
            get
            {
                return _resultIsUpdatable;
            }
            set
            {
                _resultIsUpdatable = value;
                try
                {
                    if (bufferedResult.Buffer.Count() != SG_Result.RowsCount - 1)
                    {
                        ResultFillFromBuffer(null);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private bool isRunning
        {
            get {
                try
                {
                    bool isRunning = false;
                    for (int i = 0; i < mySearchTasks.Count(); i++)
                    {
                        isRunning = isRunning || mySearchTasks[i].isRunning;
                        if (isRunning)
                        {
                            return true;
                        }
                    }
                    return isRunning;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private List<IPAddress> ipList { get; set; }
        private IPAddress firstIpAddress;
        private IPAddress lastIpAddress;
        private BufferedResult<IPInfo> bufferedResult { get; set; }
        private int TimeOut { get; set; }
        private ListIPInfo oldLines { get; set; }
        private int pictureBox1MouseLastX { get; set; }
        private List<KeyValuePair<string, IComparer>> gridHeaders { get ; set; }        

        public IPScanForm()
        {
            InitializeComponent();
            fill = new Base.IP.Grid.Fill();
            gridHeaders = new List<KeyValuePair<string, IComparer>>() {
                    new KeyValuePair<string, IComparer>("IP Address", new IPAddressComparer()),
                    new KeyValuePair<string, IComparer>("Trip Time", null),
                    new KeyValuePair<string, IComparer>("Host Name", null),
                    new KeyValuePair<string, IComparer>("Host MacAddress", new PhysicalAddressComparer())
                };
            button_Pause.Tag = false;
            //SG_Result.Controller.AddController(new GridController(Color.LightBlue));
            fill.GridFill(SG_Result, null as ListIPInfo, null, gridHeaders);
        }

        private void StartButtonEnable(bool Enable)
        {
            SetControlPropertyThreadSafe(button_Start, "Enabled", Enable);
        }
        private void StopButtonEnable(bool Enable)
        {
            SetControlPropertyThreadSafe(button_Stop, "Enabled", Enable);
            SetControlPropertyThreadSafe(button_Pause, "Enabled", Enable);            
        }              

        private void ResultFillFromBuffer(object Buffer = null)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object>(ResultFillFromBuffer), new object[] { bufferedResult });
                    return;
                }
                try
                {                    
                    fill.GridUpdateOrInsertRows(SG_Result, bufferedResult.Buffer,
                        (IPInfo item, Color color) =>
                        {
                            return new Grid.GridCellController(Color.LightBlue);
                        }
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
                    Debug.WriteLine(bufferedResult.getBufferTotalCount.ToString());
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void BufferResultAddLine(IPInfo Line)
        {
            bufferedResult.AddLine(Line);
        }

        private void DisposeTasks(object Buffer)
        {
            //bufferResult = null;
            //mySearchTasks = null;
            myTasks = null;
            //ipList = null;
            checkTasks = null;
            oldLines = null;
            GC.Collect();
        }        

        private void SetProgress(int Progress, int Thread4IpCount, int Thread4HostNameCount, TimeSpan timePassed, TimeSpan timeLeft, int pauseTime)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int, int, int, TimeSpan, TimeSpan, int>(SetProgress), Progress, Thread4IpCount, Thread4HostNameCount, timePassed, timeLeft, pauseTime);
                return;
            }
            try
            {
                toolStripProgressBar1.Value = Progress;
                label_Progress.Text = Progress.ToString() + @"\" + toolStripProgressBar1.Maximum.ToString() + "  [ " + string.Format("{0:hh\\:mm\\:ss}", timePassed) + @" \ " + string.Format("{0:hh\\:mm\\:ss}", timeLeft) + " ]";
                tSSL_Found.Text = bufferedResult.Buffer.Count().ToString();
                tSSL_ThreadIPWorks.Text = Thread4IpCount.ToString();
                tSSL_ThreadsDNS.Text = Thread4HostNameCount.ToString();
                tSSL_pauseTime.Text = pauseTime.ToString();
                
                DrawMultiProgress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
        }
        private void SetProgressMaxValue(int MaxValue)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(SetProgressMaxValue), MaxValue);
                return;
            }
            try
            {
                toolStripProgressBar1.Maximum = MaxValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
        }

        private void DrawMultiProgress_old()
        {
            Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            Graphics graphics = Graphics.FromImage(bmp);

            Pen pen = new Pen(Color.Green);
            Brush brush = Brushes.Green;

            List<IPInfo> buffer = bufferedResult.Buffer;

            if (buffer != null)
            {
                /*
                for (int i = 0; i < buffer.Count; i++)
                {
                    int index = ipList.FindIndex(IPAddress => IPAddress == buffer[i].IPAddress);
                    {
                        int x0 = (int)((double)index * bmp.Width / ipList.Count);
                        int width = 1;

                        Rectangle rectangle = new Rectangle(x0, 0, width == 0 ? 1 : width, bmp.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }
                */
                pen = new Pen(Color.Lime);
                brush = Brushes.Lime;
                int rectWidth = bmp.Width / ipList.Count;
                foreach (IPInfo item in buffer)
                {                    
                    int index = ipList.FindIndex(IPAddress => IPAddress.ToString() == item.IPAddress.ToString());
                    int x = (int)((double)index * bmp.Width / ipList.Count);
                    if (rectWidth < 2)
                    {
                        graphics.DrawLine(pen, new Point(x, 0), new Point(x, bmp.Height));
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle(x, 0, rectWidth, bmp.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }
            }

            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
        }

        private void DrawMultiProgress()
        {
            Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            Graphics graphics = Graphics.FromImage(bmp);

            Pen pen = new Pen(Color.Green);
            Brush brush = Brushes.Green;

            if (mySearchTasks != null)
            {
                for (int i = 0; i < mySearchTasks.Count; i++)
                {
                    Dictionary<int, int> progress = mySearchTasks[i].Progress;
                    foreach (int index in progress.Keys)
                    {
                        int x0 = (int)((double)index * bmp.Width / ipList.Count);
                        int x1 = (int)((double)progress[index] * bmp.Width / ipList.Count);
                        int width = x1 - x0;

                        Rectangle rectangle = new Rectangle(x0, 0, width == 0 ? 1 : width, bmp.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }

                pen = new Pen(Color.Lime);
                brush = Brushes.Lime;
                int rectWidth = bmp.Width / ipList.Count;
                foreach (IPInfo item in bufferedResult.Buffer)
                {
                    int index = ipList.FindIndex(IPAddress => IPAddress.ToString() == item.IPAddress.ToString());
                    int x = (int)((double)index * bmp.Width / ipList.Count);
                    if (rectWidth < 2)
                    {
                        graphics.DrawLine(pen, new Point(x, 0), new Point(x, bmp.Height));
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle(x, 0, rectWidth, bmp.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }
            }

            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
        }


        /*Start*/
        private void button_Start_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {

                #region Перевірка правильності введення ip адрес

                button_Pause.Tag = false;
                button_Pause.Text = "Pause";                

                //https://toster.ru/q/140605
                if (String.IsNullOrWhiteSpace(textBox_IPFirst.Text))
                {
                    MessageBox.Show("Помилка у початковій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (String.IsNullOrWhiteSpace(textBox_IPLast.Text))
                {
                    MessageBox.Show("Помилка у кінцевій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    firstIpAddress = IPAddress.Parse(textBox_IPFirst.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Помилка у початковій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    lastIpAddress = IPAddress.Parse(textBox_IPLast.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Помилка у кінцевій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }                

                try
                {
                    TimeOut = int.Parse(textBox_Timeout.Text);
                }
                catch (Exception)
                {
                    TimeOut = 100;
                }
                Console.WriteLine(TimeOut);

                #endregion

                ipList = IPAddressesRange(firstIpAddress, lastIpAddress);
                try
                {
                    SetProgressMaxValue(ipList.Count);
                    SetProgress(0, 0, 0, TimeSpan.MinValue, TimeSpan.MinValue, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                    return;
                }

                if (ipList == null || ipList.Count == 0)
                {
                    MessageBox.Show("Помилка у кінцевій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                StartButtonEnable(false);
                StopButtonEnable(true);
                button_Stop.Focus();

                bufferedResult = new BufferedResult<IPInfo>();                
                fill.GridFill(SG_Result, null as ListIPInfo, null, gridHeaders);
                oldLines = new ListIPInfo();
                pictureBox1.Image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

               
                int taskCount = 1;
                try
                {
                    taskCount = int.Parse(textBox_ThreadCount.Text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }


                List<Task> myTasks = new List<Task>();//[taskCount];
                mySearchTasks = new List<ISearchTask<IPInfo, IPAddress>>();//[taskCount];

                checkTasks = new CheckSearchTask<IPInfo, IPAddress>(
                    myTasks,
                    mySearchTasks,
                    StartButtonEnable,
                    StopButtonEnable,
                    ResultFillFromBuffer,
                    DisposeTasks,
                    SetProgress,
                    ipList.Count,
                    bufferedResult);
                newTask(checkTasks.Check);
                mySearchTasksCancel = new CancellationTokenSource();
                
                int range = (int)Math.Truncate((double)ipList.Count / taskCount);
                for (int i = 0; i < taskCount; i++)
                {
                    int count = i == taskCount - 1 ? ipList.Count - range * i : range;
                    mySearchTasks.Add(new IPSearchTask(i, ipList, i * range, count, BufferResultAddLine, TimeOut, mySearchTasksCancel.Token, checkTasks));
                    Console.WriteLine(i + ": " + i * range + ", " + (i == taskCount - 1 ? ipList.Count - range * i : range));
                    myTasks.Add(Task.Factory.StartNew(mySearchTasks[i].Start));
                }                 
            }
        }

        private async void newTask(Action action)
        {
            await Task.Run(() => { new Task(action).Start(); });
        }

        #region SetPropertyThreadSafeDelegate
        private delegate void SetPropertyThreadSafeDelegate<TResult>(
            Control @this,
            Expression<Func<TResult>> property,
            TResult value
        );
        private delegate void SetControlPropertyThreadSafeDelegate(
            Control control, string propertyName, object propertyValue
        );
        public static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate
                (SetControlPropertyThreadSafe),
                new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { propertyValue });
            }
        }

        private delegate object getControlPropertyThreadSafeDelegate(Control control, string propertyName);
        public static object getControlPropertyThreadSafe(Control control, string propertyName)
        {
            if (control.InvokeRequired)
            {
                return (object)(control.Invoke(new getControlPropertyThreadSafeDelegate(getControlPropertyThreadSafe), new object[] { control, propertyName }));
            }
            else
            {
                return (object)(control.GetType().InvokeMember(propertyName, BindingFlags.GetProperty, null, control, null));
            }
        }
        #endregion

        private static List<IPAddress> IPAddressesRange(IPAddress firstIPAddress, IPAddress lastIPAddress)
        {
            var firstIPAddressAsBytesArray = firstIPAddress.GetAddressBytes();
            var lastIPAddressAsBytesArray = lastIPAddress.GetAddressBytes();
            Array.Reverse(firstIPAddressAsBytesArray);
            Array.Reverse(lastIPAddressAsBytesArray);
            var firstIPAddressAsInt = BitConverter.ToInt32(firstIPAddressAsBytesArray, 0);
            var lastIPAddressAsInt = BitConverter.ToInt32(lastIPAddressAsBytesArray, 0);
            var ipAddressesInTheRange = new List<IPAddress>();
            for (var i = firstIPAddressAsInt; i <= lastIPAddressAsInt; i++)
            {
                var bytes = BitConverter.GetBytes(i);
                var newIp = new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] });
                ipAddressesInTheRange.Add(newIp);
            }
            return ipAddressesInTheRange;
        }


        private void button_Stop_Click(object sender, EventArgs e)
        {            
            for (int i = 0; i < mySearchTasks.Count(); i++)
            {
                try
                {                    
                    mySearchTasksCancel.Cancel();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
                Console.WriteLine(i + ": is stopped");
            }
            checkTasks.Stop();
        }

        private void button_Pause_Click(object sender, EventArgs e)
        {
            /*
            for (int i = 0; i < mySearchTasks.Count(); i++)
            {
                mySearchTasks[i].Pause();
            }
            */
            button_Pause.Tag = checkTasks.Pause();
            if ((bool)button_Pause.Tag)
            {
                button_Pause.Text = "Resume";
            }
            else
            {
                button_Pause.Text = "Pause";
            }
        }        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                button_Stop_Click(sender, null);
            }
            catch (Exception)
            {
            }
            finally
            {
                e.Cancel = false;
            }            
        }

        private int pictureBox12ipListIndex(int X)
        {
            try
            {
                double k = ipList.Count / (double)pictureBox1.Image.Width;
                return (int)( k * X );
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (pictureBox1MouseLastX != e.X) 
                {
                    pictureBox1MouseLastX = e.X;
                    int index = pictureBox12ipListIndex(e.X);
                    foreach (var Row in SG_Result.Rows)
                    {
                        int rowIndex = Row.Index;
                        SourceGrid.Cells.ICellVirtual[] cellsAtRow = Row.Grid.GetCellsAtRow(rowIndex);
                        if (cellsAtRow[0].ToString() == ipList[index].ToString())
                        {
                            SG_Result.ShowCell(new SourceGrid.Position(rowIndex, 0), true);
                            toolTip1.Show("\t" + (cellsAtRow[1].ToString() ?? "") + "\r" + (cellsAtRow[2].ToString() ?? "") + "\r" + ipList[index].ToString(), pictureBox1 as IWin32Window);
                            return;
                        }
                    }
                    toolTip1.Show("\r\r" + ipList[index].ToString(), pictureBox1 as IWin32Window);
                }
            }
            catch (Exception)
            {
                pictureBox1MouseLastX = -1;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(sender as IWin32Window);
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                List<IPInfo> ipInfoList = bufferedResult.Buffer;
                for (int i = 0; i < ipInfoList.Count; i++)
                {
                    if (ipInfoList[i].IPAddress.ToString() == ipList[pictureBox12ipListIndex(pictureBox1MouseLastX)].ToString())
                    {
                        ipInfoList[i].ShowHostForm();
                        return;
                    }
                } 
            }
            catch (Exception)
            {
            }
            
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                DrawMultiProgress();
            }
        }
    }

    
}
