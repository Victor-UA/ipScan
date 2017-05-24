namespace ipScan.Base.IP.Host
{
    partial class HostForm
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
            this.SG_HostOpenPorts = new SourceGrid.Grid();
            this.btn_ScanHostPorts = new System.Windows.Forms.Button();
            this.label_ScanPortsProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SG_HostOpenPorts
            // 
            this.SG_HostOpenPorts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SG_HostOpenPorts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SG_HostOpenPorts.AutoStretchColumnsToFitWidth = true;
            this.SG_HostOpenPorts.ClipboardMode = SourceGrid.ClipboardMode.Copy;
            this.SG_HostOpenPorts.ColumnsCount = 2;
            this.SG_HostOpenPorts.EnableSort = true;
            this.SG_HostOpenPorts.FixedRows = 1;
            this.SG_HostOpenPorts.Location = new System.Drawing.Point(12, 12);
            this.SG_HostOpenPorts.Name = "SG_HostOpenPorts";
            this.SG_HostOpenPorts.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.SG_HostOpenPorts.SelectionMode = SourceGrid.GridSelectionMode.Row;
            this.SG_HostOpenPorts.Size = new System.Drawing.Size(200, 446);
            this.SG_HostOpenPorts.TabIndex = 0;
            this.SG_HostOpenPorts.TabStop = true;
            this.SG_HostOpenPorts.ToolTipText = "";
            // 
            // btn_ScanHostPorts
            // 
            this.btn_ScanHostPorts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_ScanHostPorts.Location = new System.Drawing.Point(67, 477);
            this.btn_ScanHostPorts.Name = "btn_ScanHostPorts";
            this.btn_ScanHostPorts.Size = new System.Drawing.Size(75, 23);
            this.btn_ScanHostPorts.TabIndex = 4;
            this.btn_ScanHostPorts.Text = "Scan Ports";
            this.btn_ScanHostPorts.UseVisualStyleBackColor = true;
            this.btn_ScanHostPorts.Click += new System.EventHandler(this.btn_ScanHostPorts_Click);
            // 
            // label_ScanPortsProgress
            // 
            this.label_ScanPortsProgress.AutoSize = true;
            this.label_ScanPortsProgress.Location = new System.Drawing.Point(12, 461);
            this.label_ScanPortsProgress.Name = "label_ScanPortsProgress";
            this.label_ScanPortsProgress.Size = new System.Drawing.Size(0, 13);
            this.label_ScanPortsProgress.TabIndex = 5;
            // 
            // HostForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 512);
            this.Controls.Add(this.label_ScanPortsProgress);
            this.Controls.Add(this.btn_ScanHostPorts);
            this.Controls.Add(this.SG_HostOpenPorts);
            this.MinimumSize = new System.Drawing.Size(650, 550);
            this.Name = "HostForm";
            this.Text = "Host";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HostForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SourceGrid.Grid SG_HostOpenPorts;
        private System.Windows.Forms.Button btn_ScanHostPorts;
        private System.Windows.Forms.Label label_ScanPortsProgress;
    }
}