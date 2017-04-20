using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ipScan.Classes;

namespace ipScan
{
    public partial class Form1 : Form
    {
        private Task[] myTasks { get; set; }
        private SearchTask[] mySearchTasks { get; set; }
        private CheckTasks checkTasks { get; set; }
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
        private void ResultAddRow(string row)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ResultAddRow), new object[] { row });
                return;
            }
            richTextBox_result.AppendText(row + Environment.NewLine);
        }

        private void ResultAppendBuffer(object Buffer)
        {
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action<object>(ResultAppendBuffer), new object[] { bufferResult });
                    return;
                }
                try
                {
                    richTextBox_result.Lines = bufferResult.getAllBufferSorted().toArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
                    Console.WriteLine(bufferResult.getBufferTotalCount.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
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
            bufferResult = null;
            mySearchTasks = null;
            myTasks = null;
            ipList = null;
            checkTasks = null;
            oldLines = null;
            System.GC.Collect();
        }        
        private void SetProgress(int Progress, int Thread4IpCount, ListIPInfo IpArePassed, ListIPInfo IpAreFound, int Thread4HostNameCount, ListIPInfo IpAreLooking4HostName, TimeSpan timePassed, TimeSpan timeLeft)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int, int, ListIPInfo, ListIPInfo, int, ListIPInfo, TimeSpan, TimeSpan>(SetProgress), Progress, Thread4IpCount, IpArePassed, IpAreFound, Thread4HostNameCount, IpAreLooking4HostName, timePassed, timeLeft);
                return;
            }
            try
            {
                toolStripProgressBar1.Value = Progress;
                label_Progress.Text = Progress.ToString() + @"\" + toolStripProgressBar1.Maximum.ToString() + "  [ " + string.Format("{0:hh\\:mm\\:ss}", timePassed) + @" \ " + string.Format("{0:hh\\:mm\\:ss}", timeLeft) + " ]";
                tSSL_Found.Text = richTextBox_result.Lines.Count().ToString();
                tSSL_ThreadIPWorks.Text = Thread4IpCount.ToString();
                tSSL_ThreadsDNS.Text = Thread4HostNameCount.ToString();
                
                //DrawMultiProgress(IpAreLooking4HostName, Color.Yellow);                
                DrawMultiProgress(IpArePassed, Color.Green);
                DrawMultiProgress(IpAreFound, Color.Lime);
                //DrawMultiProgress(bufferResult.Buffer, Color.Lime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
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
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
        }

        private void DrawMultiProgress(ListIPInfo Buffer, Color color)
        {
            if (Buffer != null && Buffer.Count > 0)
            {                
                Graphics graphics = Graphics.FromImage(pictureBox1.Image);
                Pen pen = new Pen(color);
                Brush brush = color == Color.Green ? Brushes.Green : color == Color.Lime ? Brushes.Lime : Brushes.Yellow;
                
                int x = 0;
                int rectWidth = pictureBox1.Image.Width / ipList.Count;
                foreach (IPInfo item in Buffer)
                {
                    int index = ipList.FindIndex(IPAddress => IPAddress == item.IPAddress);
                    x = (index * pictureBox1.Image.Width) / ipList.Count;
                    if (rectWidth < 2)
                    {
                        graphics.DrawLine(pen, new Point(x, 0), new Point(x, pictureBox1.Image.Height));
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle(x, 0, rectWidth, pictureBox1.Image.Height);
                        graphics.DrawRectangle(pen, rectangle);
                        graphics.FillRectangle(brush, rectangle);
                    }
                }
                pictureBox1.Refresh();
            }
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
                richTextBox_result.Clear();
                pictureBox1.Image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);

                ipList = IPAddressesRange(firstIpAddress, lastIpAddress);
                try
                {
                    SetProgressMaxValue(ipList.Count);
                    SetProgress(0, 0, null, null, 0, null, TimeSpan.MinValue, TimeSpan.MinValue);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }

                int taskCount = 1;
                try
                {
                    taskCount = int.Parse(textBox_ThreadCount.Text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }


                myTasks = new Task[taskCount];
                mySearchTasks = new SearchTask[taskCount];

                checkTasks = new CheckTasks(
                    myTasks,
                    mySearchTasks,
                    StartButtonEnable,
                    StopButtonEnable,
                    ResultAppendBuffer,
                    DisposeTasks,
                    SetProgress,
                    ipList.Count,
                    bufferResult);
                Task.Factory.StartNew(checkTasks.Check);

                int range = (int)Math.Truncate((double)ipList.Count / taskCount);
                for (int i = 0; i < taskCount; i++)
                {
                    mySearchTasks[i] = new SearchTask(i, ipList, i * range, i == taskCount - 1 ? ipList.Count - range * i : range, BufferResultAddLine, TimeOut);
                    Console.WriteLine(i + ": " + i * range + ", " + (i == taskCount - 1 ? ipList.Count - range * i : range));
                    myTasks[i] = Task.Factory.StartNew(mySearchTasks[i].Start);
                }
                
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
            if (isRunning)
            {
                for (int i = 0; i < mySearchTasks.Count(); i++)
                {
                    mySearchTasks[i].Stop();
                    Console.WriteLine(i + ": is stopped");
                }

                checkTasks.Stop();
            }
        }

        private void richTextBox_result_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void button_Pause_Click(object sender, EventArgs e)
        {
            
            for (int i = 0; i < mySearchTasks.Count(); i++)
            {
                mySearchTasks[i].Pause();
            }
            checkTasks.Pause();
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
    }

    
}
