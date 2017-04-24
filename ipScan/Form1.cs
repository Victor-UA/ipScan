using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ipScan.Classes;

namespace ipScan
{
    public partial class Form1 : Form
    {
        private List<Task> myTasks { get; set; }
        private List<SearchTask> mySearchTasks { get; set; }
        private CancellationTokenSource mySearchTasksCancel { get; set; }
        private CheckTasks checkTasks { get; set; }
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
                    if (bufferResult.Buffer.Count() != SourceGrid_Result.RowsCount)
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
                catch(Exception)
                {
                    return false;
                }
            }
        }
        private List<IPAddress> ipList { get; set; }
        private IPAddress firstIpAddress;
        private IPAddress lastIpAddress;
        private BufferResult bufferResult { get; set; }
        private int TimeOut { get; set; }
        private ListIPInfo oldLines { get; set; }

        public Form1()
        {
            InitializeComponent();
            GridFill(SourceGrid_Result, null);
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

        private void GridFill(SourceGrid.Grid grid, ListIPInfo IPList, List<string> Fields = null, Dictionary<string, object> filter = null, string filtertype = "", bool casesensitive = false)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            List<string> fields = Fields == null ? 
                new List<string>() { "IP Address", "TTL", "Host Name"} : 
                Fields;

            //Columns filling
            grid.ColumnsCount = fields.Count;
            grid.FixedRows = 1;
            grid.Rows.Insert(0);            
            for (int i = 0; i < (fields.Count); i++)
            {
                grid[0, i] = new SourceGrid.Cells.ColumnHeader(fields[i]);
            }

            //Data filling
            if (IPList != null) { 
                int index = grid.RowsCount;
                for (int r = 0; r < IPList.Count; r++)
                {
                    grid.Rows.Insert(index++);
                    try
                    {
                        grid.Rows[grid.RowsCount - 1].Tag = new RowTag(r, IPList[r]);
                    }
                    catch (Exception ex)
                    {
                        grid.Rows[grid.RowsCount - 1].Tag = new RowTag(r, null);
                        Debug.WriteLine(ex.StackTrace);
                    }

                    grid[grid.RowsCount - 1, 0] = new SourceGrid.Cells.Cell(IPList[r].IPAddress);
                    grid[grid.RowsCount - 1, 1] = new SourceGrid.Cells.Cell(IPList[r].RoundtripTime);
                    grid[grid.RowsCount - 1, 2] = new SourceGrid.Cells.Cell(IPList[r].HostName);
                }
            }
            grid.AutoSizeCells();
        }  

        private void ResultFillFromBuffer(object Buffer = null)
        {
            try
            {
                if (resultIsUpdatable)
                {
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action<object>(ResultFillFromBuffer), new object[] { bufferResult });
                        return;
                    }
                    try
                    {
                        GridFill(SourceGrid_Result, bufferResult.getAllBufferSorted());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
                        Debug.WriteLine(bufferResult.getBufferTotalCount.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }
        private void BufferResultAddLine(IPInfo Line)
        {
            bufferResult.AddLine(Line);
        }
        private void BufferResultAddLines(ListIPInfo Lines)
        {
            bufferResult.AddLines(Lines);
        }
        private void DisposeTasks(object Buffer)
        {
            //bufferResult = null;
            mySearchTasks = null;
            myTasks = null;
            ipList = null;
            checkTasks = null;
            oldLines = null;
            System.GC.Collect();
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
                tSSL_Found.Text = bufferResult.Buffer.Count().ToString();
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

        private void DrawMultiProgress()
        {
            Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            Graphics graphics = Graphics.FromImage(bmp);

            Pen pen = new Pen(Color.Green);
            Brush brush = Brushes.Green;

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
            foreach (IPInfo item in bufferResult.getAllBuffer())
            {
                int index = ipList.FindIndex(IPAddress => IPAddress == item.IPAddress);
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

            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
        }


        /*Start*/
        private void button_Start_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {                

                #region Перевірка правильності введення ip адрес

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

                StartButtonEnable(false);
                StopButtonEnable(true);

                bufferResult = new BufferResult();
                oldLines = new ListIPInfo();
                pictureBox1.Image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

                ipList = IPAddressesRange(firstIpAddress, lastIpAddress);
                try
                {
                    SetProgressMaxValue(ipList.Count);
                    SetProgress(0, 0, 0, TimeSpan.MinValue, TimeSpan.MinValue, 0);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }

                int taskCount = 1;
                try
                {
                    taskCount = int.Parse(textBox_ThreadCount.Text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
                
                myTasks = new List<Task>();//[taskCount];
                mySearchTasks = new List<SearchTask>();//[taskCount];

                checkTasks = new CheckTasks(
                    myTasks,
                    mySearchTasks,
                    StartButtonEnable,
                    StopButtonEnable,
                    ResultFillFromBuffer,
                    DisposeTasks,
                    SetProgress,
                    ipList.Count,
                    bufferResult);
                //Task.Factory.StartNew(checkTasks.Check);
                newTask(checkTasks.Check);
                mySearchTasksCancel = new CancellationTokenSource();
                /*
                int range = (int)Math.Truncate((double)ipList.Count / taskCount);
                for (int i = 0; i < taskCount; i++)
                {
                    int count = i == taskCount - 1 ? ipList.Count - range * i : range;
                    mySearchTasks.Add(new SearchTask(i, ipList.GetRange(i * range, count), 0, count, BufferResultAddLine, TimeOut, mySearchTasksCancel.Token, checkTasks));
                    Console.WriteLine(i + ": " + i * range + ", " + (i == taskCount - 1 ? ipList.Count - range * i : range));
                    myTasks.Add(Task.Factory.StartNew(mySearchTasks[i].Start));
                }
                */
                
                int range = (int)Math.Truncate((double)ipList.Count / taskCount);
                for (int i = 0; i < taskCount; i++)
                {
                    int count = i == taskCount - 1 ? ipList.Count - range * i : range;
                    mySearchTasks.Add(new SearchTask(i, ipList, i * range, count, BufferResultAddLine, TimeOut, mySearchTasksCancel.Token, checkTasks));
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

        private void richTextBox_result_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void button_Pause_Click(object sender, EventArgs e)
        {
            /*
            for (int i = 0; i < mySearchTasks.Count(); i++)
            {
                mySearchTasks[i].Pause();
            }
            */
            checkTasks.Pause();
            if ((bool)button_Pause.Tag)
            {
                button_Pause.Tag = false;
                button_Pause.Text = "Resume";
            }
            else
            {
                button_Pause.Tag = true;
                button_Pause.Text = "Pause";
            }
        }

        private void richTextBox_result_Enter(object sender, EventArgs e)
        {
            try
            {
                checkTasks.BlockResultOutput(true);
            }
            catch (Exception) { }
        }

        private void richTextBox_result_Leave(object sender, EventArgs e)
        {
            try
            {
                checkTasks.BlockResultOutput(false);
            }
            catch (Exception) { }
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

        private void richTextBox_result_MouseDown(object sender, MouseEventArgs e)
        {
            resultIsUpdatable = false;
        }

        private void richTextBox_result_MouseUp(object sender, MouseEventArgs e)
        {
            resultIsUpdatable = true;
        }
    }

    
}
