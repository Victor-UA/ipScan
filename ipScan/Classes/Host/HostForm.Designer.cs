namespace ipScan.Classes.Host
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
            this.grid_HostOpenPorts = new SourceGrid.Grid();
            this.btn_ScanHostPorts = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // grid_HostOpenPorts
            // 
            this.grid_HostOpenPorts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grid_HostOpenPorts.AutoStretchColumnsToFitWidth = true;
            this.grid_HostOpenPorts.ClipboardMode = SourceGrid.ClipboardMode.Copy;
            this.grid_HostOpenPorts.ColumnsCount = 2;
            this.grid_HostOpenPorts.EnableSort = true;
            this.grid_HostOpenPorts.FixedRows = 1;
            this.grid_HostOpenPorts.Location = new System.Drawing.Point(12, 12);
            this.grid_HostOpenPorts.Name = "grid_HostOpenPorts";
            this.grid_HostOpenPorts.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.grid_HostOpenPorts.FixedRows = 1;
            this.grid_HostOpenPorts.SelectionMode = SourceGrid.GridSelectionMode.Row;
            this.grid_HostOpenPorts.Size = new System.Drawing.Size(120, 439);
            this.grid_HostOpenPorts.TabIndex = 0;
            this.grid_HostOpenPorts.TabStop = true;
            this.grid_HostOpenPorts.ToolTipText = "";            
            // 
            // btn_ScanHostPorts
            // 
            this.btn_ScanHostPorts.Location = new System.Drawing.Point(32, 457);
            this.btn_ScanHostPorts.Name = "btn_ScanHostPorts";
            this.btn_ScanHostPorts.Size = new System.Drawing.Size(75, 23);
            this.btn_ScanHostPorts.TabIndex = 4;
            this.btn_ScanHostPorts.Text = "Scan Ports";
            this.btn_ScanHostPorts.UseVisualStyleBackColor = true;
            this.btn_ScanHostPorts.Click += new System.EventHandler(this.btn_ScanHostPorts_Click);
            // 
            // HostForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 492);
            this.Controls.Add(this.btn_ScanHostPorts);
            this.Controls.Add(this.grid_HostOpenPorts);
            this.Name = "HostForm";
            this.Text = "Host";
            this.ResumeLayout(false);

        }

        #endregion

        private SourceGrid.Grid grid_HostOpenPorts;
        private System.Windows.Forms.Button btn_ScanHostPorts;
    }
}