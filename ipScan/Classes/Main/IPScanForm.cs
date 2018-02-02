using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ipScan.Base;
using ipScan.Base.IP;
using System.Collections;
using System.Net.NetworkInformation;
using ipScan.Properties;
using System.Collections.Concurrent;

namespace ipScan.Classes.Main
{
    public partial class IPScanForm : Form
    {
        private Base.IP.Grid.Fill _fill;
        private List<Task<PingReply>> _myTasks { get; set; }
        private List<ISearchTask<IPInfo, uint>> _mySearchTasks { get; set; }
        private CancellationTokenSource _mySearchTasksCancel { get; set; }
        private TasksChecking<IPInfo, uint> _tasksChecking { get; set; }
        private bool _resultIsUpdatable = true;
        private bool ResultIsUpdatable
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
                    if (_bufferedResult.Buffer.Count() != SG_Result.RowsCount - 1)
                    {
                        ResultFillFromBuffer(null);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private bool _isRunning
        {
            get {
                try
                {
                    bool isRunning = false;
                    if (_mySearchTasks != null)
                    {
                        for (int i = 0; i < _mySearchTasks.Count(); i++)
                        {
                            isRunning = isRunning || _mySearchTasks[i].IsRunning;
                            if (isRunning)
                            {
                                return true;
                            }
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
        private uint _ipListCount { get; set; }
        private uint _firstIpAddress;
        private uint _lastIpAddress;
        private BufferedResult<IPInfo> _bufferedResult { get; set; }
        private int _timeOut { get; set; }
        private ListIPInfo _oldLines { get; set; }
        private int _pictureBox1MouseLastX { get; set; }
        private List<KeyValuePair<string, IComparer>>   _gridHeaders { get ; set; }  
        private Bitmap                                  _bmpTasksResult { get; set; }
        private int bmpWidth = 5000;
        private int bmpHeight = 19;



        public IPScanForm()
        {
            InitializeComponent();
            _fill = new Base.IP.Grid.Fill();
            _gridHeaders = new List<KeyValuePair<string, IComparer>>() {
                    new KeyValuePair<string, IComparer>("IP Address", new IPAddressComparer()),
                    new KeyValuePair<string, IComparer>("Trip Time", null),
                    new KeyValuePair<string, IComparer>("Host Name", null),
                    new KeyValuePair<string, IComparer>("Host MacAddress", new PhysicalAddressComparer())
                };
            button_Pause.Tag = false;
            //SG_Result.Controller.AddController(new GridController(Color.LightBlue));
            _fill.GridFill(SG_Result, null as ListIPInfo, null, _gridHeaders);
        }
        private void IPScanForm_Load(object sender, EventArgs e)
        {
            if (Settings.Default.MainWindowLocation != null)
            {
                Location = Settings.Default.MainWindowLocation;
            }
            if (Settings.Default.MainWindowSize != null)
            {
                Size = Settings.Default.MainWindowSize;
            }
            if (Settings.Default.MainWindowState != FormWindowState.Minimized)
            {
                WindowState = Settings.Default.MainWindowState;
            }
        }
        private void IPScanForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.MainWindowLocation = Location;
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.MainWindowSize = Size;
            }
            else
            {
                Settings.Default.MainWindowSize = RestoreBounds.Size;
            }
            Settings.Default.MainWindowState = WindowState;
            Settings.Default.Save();

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
                    Invoke(new Action<object>(ResultFillFromBuffer), new object[] { _bufferedResult });
                    return;
                }
                try
                {                    
                    _fill.GridUpdateOrInsertRows(SG_Result, _bufferedResult.Buffer,
                        (IPInfo item, Color color) =>
                        {
                            return new Grid.GridCellController(Color.LightBlue);
                        }
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
                    Debug.WriteLine(_bufferedResult.getBufferTotalCount.ToString());
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void BufferResultAddLine(IPInfo Line)
        {
            _bufferedResult.AddLine(Line);
        }

        private void DisposeTasks(object Buffer)
        {
            //bufferResult = null;
            //mySearchTasks = null;
            _myTasks = null;
            //ipList = null;
            _tasksChecking = null;
            _oldLines = null;
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
                label_Progress.Text = Progress.ToString() + @"\" + _ipListCount.ToString() + "  [ " + string.Format("{0:hh\\:mm\\:ss}", timePassed) + @" \ " + string.Format("{0:hh\\:mm\\:ss}", timeLeft) + " ]";
                tSSL_Found.Text = _bufferedResult == null ? "0" : _bufferedResult.Buffer.Count().ToString();
                tSSL_ThreadIPWorks.Text = Thread4IpCount.ToString();
                tSSL_ThreadsDNS.Text = Thread4HostNameCount.ToString();
                tSSL_pauseTime.Text = pauseTime.ToString();
                toolStripProgressBar1.Value = (int)(Progress * 100 / _ipListCount);
                
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

        private void DrawMultiProgress()
        {
            Bitmap bmpTasksProgress = new Bitmap(bmpWidth, bmpHeight);
            Graphics graphics = Graphics.FromImage(bmpTasksProgress);

            Pen pen = new Pen(Color.Green);
            Brush brush = Brushes.Green;

            if (_mySearchTasks != null)
            {
                foreach (var mySearchTask in _mySearchTasks)
                {                    
                    foreach (uint index in mySearchTask.ProgressDict.Keys)
                    {
                        int x0 = (int)((index - _firstIpAddress) * (uint)bmpTasksProgress.Width / _ipListCount);
                        int x1 = (int)((mySearchTask.ProgressDict[index] - _firstIpAddress) * (uint)bmpTasksProgress.Width / _ipListCount);
                        int width = x1 - x0;

                        Rectangle rectangle = new Rectangle(x0, 0, width == 0 ? 1 : width, bmpTasksProgress.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }                

                graphics = Graphics.FromImage(_bmpTasksResult);
                pen = new Pen(Color.Lime);
                brush = Brushes.Lime;
                int rectWidth = (int)(_bmpTasksResult.Width / _ipListCount);
                List<IPInfo> buffer = _bufferedResult.getBuffer();
                foreach (IPInfo item in buffer)
                {
                    uint index = item.IPAddress - _firstIpAddress;
                    int x = (int)((double)index * _bmpTasksResult.Width / _ipListCount);
                    if (rectWidth < 2)
                    {
                        graphics.DrawLine(pen, new Point(x, 0), new Point(x, _bmpTasksResult.Height));
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle(x, 0, rectWidth, _bmpTasksResult.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }

                Bitmap bmp = new Bitmap(bmpWidth, bmpHeight);
                graphics = Graphics.FromImage(bmp);
                graphics.DrawImageUnscaled(bmpTasksProgress, 0, 0);
                graphics.DrawImageUnscaled(_bmpTasksResult, 0, 0);
                pictureBox1.Image = bmp;
                pictureBox1.Refresh();
            }
        }


        /*Start*/
        private void button_Start_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
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
                    _firstIpAddress = IPTools.IPAddress2UInt32(textBox_IPFirst.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Помилка у початковій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    _lastIpAddress = IPTools.IPAddress2UInt32(textBox_IPLast.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Помилка у кінцевій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }                

                try
                {
                    _timeOut = int.Parse(textBox_Timeout.Text);
                }
                catch (Exception)
                {
                    _timeOut = 100;
                }
                Console.WriteLine(_timeOut);

                #endregion

                #region Ініціалізація інтерфейса
                _ipListCount = (_lastIpAddress - _firstIpAddress) + 1;

                _bmpTasksResult = new Bitmap(bmpWidth, bmpHeight);

                try
                {
                    SetProgressMaxValue(100);
                    SetProgress(0, 0, 0, TimeSpan.MinValue, TimeSpan.MinValue, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                    return;
                }

                if (_ipListCount == 0)
                {
                    MessageBox.Show("Помилка у кінцевій адресі", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                StartButtonEnable(false);
                StopButtonEnable(true);
                button_Stop.Focus();

                _bufferedResult = new BufferedResult<IPInfo>();                
                _fill.GridFill(SG_Result, null as ListIPInfo, null, _gridHeaders);
                _oldLines = new ListIPInfo();
                pictureBox1.Image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

                               
                int tasksCount = 1;
                try
                {
                    tasksCount = int.Parse(textBox_ThreadCount.Text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
                #endregion

                #region Create&Run Tasks

                #region Create&Run TasksChecking
                List<Task> myTasks = new List<Task>();//[taskCount]; 
                _mySearchTasks = new List<ISearchTask<IPInfo, uint>>();//[taskCount];
                _mySearchTasksCancel = new CancellationTokenSource();

                _tasksChecking = new TasksChecking<IPInfo, uint>(
                    myTasks,
                    _mySearchTasks,
                    StartButtonEnable,
                    StopButtonEnable,
                    ResultFillFromBuffer,
                    DisposeTasks,
                    SetProgress,
                    _ipListCount,
                    _bufferedResult);
                Task.Factory.StartNew(_tasksChecking.Check, _mySearchTasksCancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                #endregion                

                #region (DISABLED) Get maxTaskCount from NumberOfCores
                //try
                //{
                //    var cpu =
                //        new ManagementObjectSearcher("select * from Win32_Processor")
                //        .Get()
                //        .Cast<ManagementObject>()
                //        .First();

                //    maxTaskCount = int.Parse(cpu["NumberOfCores"].ToString()) * 2 * 100;

                //}
                //catch (Exception)
                //{
                //} 
                #endregion

                #region Create&Run IPSearchTasks
                //Get Each IPSearchTask IPAddresses range
                uint range = (uint)Math.Truncate((double)_ipListCount / tasksCount);

                if (range == 0)
                {
                    range = 1;
                    tasksCount = (int)_ipListCount;
                }

                //Create & Run IPSearchTasks
                for (int i = 0; i < tasksCount; i++)
                {
                    uint count = ((i == tasksCount - 1) ? _ipListCount - range * (uint)i : range);
                    IPSearchTask ipSearchTask = new IPSearchTask(
                        i, _firstIpAddress + (uint)i * range, count, BufferResultAddLine, 
                        _timeOut, _mySearchTasksCancel.Token, _tasksChecking
                    );
                    _mySearchTasks.Add(ipSearchTask);
                    Console.WriteLine(i + ": " + i * range + ", " +
                        (i == tasksCount - 1 ? _ipListCount - range * i : range)
                    );
                    myTasks.Add(Task.Factory.StartNew(ipSearchTask.Start, TaskCreationOptions.LongRunning));
                } 
                #endregion

                #endregion
            }
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

        private void button_Stop_Click(object sender, EventArgs e)
        {            
            for (int i = 0; i < _mySearchTasks.Count(); i++)
            {
                try
                {                    
                    _mySearchTasksCancel.Cancel();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
                Console.WriteLine(i + ": is stopped");
            }
            _tasksChecking.Stop();
        }

        private void button_Pause_Click(object sender, EventArgs e)
        {            
            button_Pause.Tag = _tasksChecking.Pause();
            if ((bool)button_Pause.Tag)
            {
                button_Pause.Text = "Resume";
            }
            else
            {
                button_Pause.Text = "Pause";
            }
        }                

        private uint pictureBox12ipListIndex(int X)
        {
            try
            {
                double k = _ipListCount / (double)pictureBox1.Width;
                return (uint)(k * X);
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
                if (_ipListCount > 0 && _pictureBox1MouseLastX != e.X) 
                {
                    _pictureBox1MouseLastX = e.X;
                    uint index = pictureBox12ipListIndex(e.X);
                    string ipAddress = IPTools.UInt322IPAddressStr(_firstIpAddress + index);
                    foreach (var Row in SG_Result.Rows)
                    {
                        int rowIndex = Row.Index;
                        SourceGrid.Cells.ICellVirtual[] cellsAtRow = Row.Grid.GetCellsAtRow(rowIndex);
                        if (cellsAtRow[0].ToString() == ipAddress) 
                        {
                            SG_Result.ShowCell(new SourceGrid.Position(rowIndex, 0), true);
                            toolTip1.Show("\t" + (cellsAtRow[1].ToString() ?? "") + "\r" + (cellsAtRow[2].ToString() ?? "") + "\r" + ipAddress, pictureBox1 as IWin32Window);
                            return;
                        }
                    }
                    toolTip1.Show("\r\r" + ipAddress, pictureBox1 as IWin32Window);
                }
            }
            catch (Exception)
            {
                _pictureBox1MouseLastX = -1;
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
                List<IPInfo> ipInfoList = _bufferedResult.Buffer;
                for (int i = 0; i < ipInfoList.Count; i++)
                {
                    if (ipInfoList[i].IPAddressStr == IPTools.UInt322IPAddressStr(_firstIpAddress + pictureBox12ipListIndex(_pictureBox1MouseLastX)))
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
            if (!_isRunning)
            {
                DrawMultiProgress();
            }
        }

        private void textBox_ThreadCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {                
                e.Handled = true;
            }
        }        
    }    
}