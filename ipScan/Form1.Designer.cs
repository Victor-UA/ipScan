namespace ipScan
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox_IPFirst = new System.Windows.Forms.TextBox();
            this.textBox_IPLast = new System.Windows.Forms.TextBox();
            this.textBox_ThreadCount = new System.Windows.Forms.TextBox();
            this.button_Start = new System.Windows.Forms.Button();
            this.button_Stop = new System.Windows.Forms.Button();
            this.richTextBox_result = new System.Windows.Forms.RichTextBox();
            this.textBox_Timeout = new System.Windows.Forms.TextBox();
            this.label_Progress = new System.Windows.Forms.Label();
            this.button_Pause = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tSSL_Found = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tSSL_ThreadIPWorks = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tSSL_ThreadsDNS = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tSSL_pauseTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_IPFirst
            // 
            this.textBox_IPFirst.Location = new System.Drawing.Point(12, 12);
            this.textBox_IPFirst.Name = "textBox_IPFirst";
            this.textBox_IPFirst.Size = new System.Drawing.Size(165, 20);
            this.textBox_IPFirst.TabIndex = 0;
            this.textBox_IPFirst.Text = "10.65.56.0";
            // 
            // textBox_IPLast
            // 
            this.textBox_IPLast.Location = new System.Drawing.Point(183, 12);
            this.textBox_IPLast.Name = "textBox_IPLast";
            this.textBox_IPLast.Size = new System.Drawing.Size(171, 20);
            this.textBox_IPLast.TabIndex = 1;
            this.textBox_IPLast.Text = "10.65.57.0";
            // 
            // textBox_ThreadCount
            // 
            this.textBox_ThreadCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_ThreadCount.Location = new System.Drawing.Point(12, 431);
            this.textBox_ThreadCount.Name = "textBox_ThreadCount";
            this.textBox_ThreadCount.Size = new System.Drawing.Size(50, 20);
            this.textBox_ThreadCount.TabIndex = 2;
            this.textBox_ThreadCount.TabStop = false;
            this.textBox_ThreadCount.Text = "64";
            // 
            // button_Start
            // 
            this.button_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Start.Location = new System.Drawing.Point(12, 512);
            this.button_Start.Name = "button_Start";
            this.button_Start.Size = new System.Drawing.Size(75, 23);
            this.button_Start.TabIndex = 4;
            this.button_Start.Text = "Start";
            this.button_Start.UseVisualStyleBackColor = true;
            this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
            // 
            // button_Stop
            // 
            this.button_Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Stop.Enabled = false;
            this.button_Stop.Location = new System.Drawing.Point(497, 512);
            this.button_Stop.Name = "button_Stop";
            this.button_Stop.Size = new System.Drawing.Size(75, 23);
            this.button_Stop.TabIndex = 6;
            this.button_Stop.Text = "Stop";
            this.button_Stop.UseVisualStyleBackColor = true;
            this.button_Stop.Click += new System.EventHandler(this.button_Stop_Click);
            // 
            // richTextBox_result
            // 
            this.richTextBox_result.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.richTextBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox_result.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox_result.Location = new System.Drawing.Point(12, 38);
            this.richTextBox_result.Name = "richTextBox_result";
            this.richTextBox_result.Size = new System.Drawing.Size(562, 387);
            this.richTextBox_result.TabIndex = 2;
            this.richTextBox_result.TabStop = false;
            this.richTextBox_result.Text = "";
            this.richTextBox_result.Enter += new System.EventHandler(this.richTextBox_result_Enter);
            this.richTextBox_result.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox_result_KeyPress);
            this.richTextBox_result.Leave += new System.EventHandler(this.richTextBox_result_Leave);
            // 
            // textBox_Timeout
            // 
            this.textBox_Timeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_Timeout.Location = new System.Drawing.Point(68, 431);
            this.textBox_Timeout.Name = "textBox_Timeout";
            this.textBox_Timeout.Size = new System.Drawing.Size(50, 20);
            this.textBox_Timeout.TabIndex = 3;
            this.textBox_Timeout.TabStop = false;
            this.textBox_Timeout.Text = "10";
            // 
            // label_Progress
            // 
            this.label_Progress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_Progress.AutoSize = true;
            this.label_Progress.Location = new System.Drawing.Point(12, 487);
            this.label_Progress.Name = "label_Progress";
            this.label_Progress.Size = new System.Drawing.Size(0, 13);
            this.label_Progress.TabIndex = 7;
            // 
            // button_Pause
            // 
            this.button_Pause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Pause.Enabled = false;
            this.button_Pause.Location = new System.Drawing.Point(416, 512);
            this.button_Pause.Name = "button_Pause";
            this.button_Pause.Size = new System.Drawing.Size(75, 23);
            this.button_Pause.TabIndex = 5;
            this.button_Pause.Tag = "true";
            this.button_Pause.Text = "Pause";
            this.button_Pause.UseVisualStyleBackColor = true;
            this.button_Pause.Click += new System.EventHandler(this.button_Pause_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tSSL_Found,
            this.toolStripStatusLabel2,
            this.tSSL_ThreadIPWorks,
            this.toolStripStatusLabel3,
            this.tSSL_ThreadsDNS,
            this.toolStripProgressBar1,
            this.toolStripStatusLabel4,
            this.tSSL_pauseTime});
            this.statusStrip1.Location = new System.Drawing.Point(0, 549);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(584, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(41, 17);
            this.toolStripStatusLabel1.Text = "Found";
            // 
            // tSSL_Found
            // 
            this.tSSL_Found.AutoSize = false;
            this.tSSL_Found.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tSSL_Found.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tSSL_Found.Name = "tSSL_Found";
            this.tSSL_Found.Size = new System.Drawing.Size(50, 17);
            this.tSSL_Found.Text = "0";
            this.tSSL_Found.ToolTipText = "Знайдено IP";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(49, 17);
            this.toolStripStatusLabel2.Text = "Threads";
            // 
            // tSSL_ThreadIPWorks
            // 
            this.tSSL_ThreadIPWorks.AutoSize = false;
            this.tSSL_ThreadIPWorks.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tSSL_ThreadIPWorks.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tSSL_ThreadIPWorks.Name = "tSSL_ThreadIPWorks";
            this.tSSL_ThreadIPWorks.Size = new System.Drawing.Size(50, 17);
            this.tSSL_ThreadIPWorks.Text = "0";
            this.tSSL_ThreadIPWorks.ToolTipText = "Потоків запущено";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(77, 17);
            this.toolStripStatusLabel3.Text = "DNS requests";
            // 
            // tSSL_ThreadsDNS
            // 
            this.tSSL_ThreadsDNS.AutoSize = false;
            this.tSSL_ThreadsDNS.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tSSL_ThreadsDNS.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tSSL_ThreadsDNS.Name = "tSSL_ThreadsDNS";
            this.tSSL_ThreadsDNS.Size = new System.Drawing.Size(50, 17);
            this.tSSL_ThreadsDNS.Text = "0";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(58, 17);
            this.toolStripStatusLabel4.Text = "loopTime";
            // 
            // tSSL_pauseTime
            // 
            this.tSSL_pauseTime.AutoSize = false;
            this.tSSL_pauseTime.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tSSL_pauseTime.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tSSL_pauseTime.Name = "tSSL_pauseTime";
            this.tSSL_pauseTime.Size = new System.Drawing.Size(50, 17);
            this.tSSL_pauseTime.Text = "0";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Location = new System.Drawing.Point(13, 458);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(559, 19);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 571);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_Pause);
            this.Controls.Add(this.label_Progress);
            this.Controls.Add(this.textBox_Timeout);
            this.Controls.Add(this.button_Stop);
            this.Controls.Add(this.button_Start);
            this.Controls.Add(this.textBox_ThreadCount);
            this.Controls.Add(this.richTextBox_result);
            this.Controls.Add(this.textBox_IPLast);
            this.Controls.Add(this.textBox_IPFirst);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 450);
            this.Name = "Form1";
            this.Text = "ipScan";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_IPFirst;
        private System.Windows.Forms.TextBox textBox_IPLast;
        private System.Windows.Forms.TextBox textBox_ThreadCount;
        private System.Windows.Forms.Button button_Start;
        private System.Windows.Forms.Button button_Stop;
        private System.Windows.Forms.RichTextBox richTextBox_result;
        private System.Windows.Forms.TextBox textBox_Timeout;
        private System.Windows.Forms.Label label_Progress;
        private System.Windows.Forms.Button button_Pause;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tSSL_Found;
        private System.Windows.Forms.ToolStripStatusLabel tSSL_ThreadIPWorks;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel tSSL_ThreadsDNS;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel tSSL_pauseTime;
    }
}

