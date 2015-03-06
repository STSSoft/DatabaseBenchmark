using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
namespace DatabaseBenchmark.Frames
{
    partial class StepFrame
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel2 = new System.Windows.Forms.Panel();
            this.LayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tabControlCharts = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lineChartAverageSpeed = new DatabaseBenchmark.Charts.LineChartFrame();
            this.lineChartMomentSpeed = new DatabaseBenchmark.Charts.LineChartFrame();
            this.lineChartAverageCPU = new DatabaseBenchmark.Charts.LineChartFrame();
            this.lineChartAverageMemory = new DatabaseBenchmark.Charts.LineChartFrame();
            this.lineChartAverageIO = new DatabaseBenchmark.Charts.LineChartFrame();
            this.barChartSpeed = new DatabaseBenchmark.Charts.BarChartFrame();
            this.barChartTime = new DatabaseBenchmark.Charts.BarChartFrame();
            this.barChartSize = new DatabaseBenchmark.Charts.BarChartFrame();
            this.barChartCPU = new DatabaseBenchmark.Charts.BarChartFrame();
            this.barChartMemory = new DatabaseBenchmark.Charts.BarChartFrame();
            this.barChartIO = new DatabaseBenchmark.Charts.BarChartFrame();
            this.LayoutPanel.SuspendLayout();
            this.tabControlCharts.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1040, 0);
            this.panel2.TabIndex = 4;
            // 
            // LayoutPanel
            // 
            this.LayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.LayoutPanel.ColumnCount = 6;
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.58846F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.58846F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.58846F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.58846F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.86662F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.77956F));
            this.LayoutPanel.Controls.Add(this.barChartSpeed, 0, 0);
            this.LayoutPanel.Controls.Add(this.barChartTime, 1, 0);
            this.LayoutPanel.Controls.Add(this.barChartSize, 2, 0);
            this.LayoutPanel.Controls.Add(this.barChartCPU, 3, 0);
            this.LayoutPanel.Controls.Add(this.barChartMemory, 4, 0);
            this.LayoutPanel.Controls.Add(this.barChartIO, 5, 0);
            this.LayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.LayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.LayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutPanel.Name = "LayoutPanel";
            this.LayoutPanel.RowCount = 1;
            this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutPanel.Size = new System.Drawing.Size(1040, 281);
            this.LayoutPanel.TabIndex = 9;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 281);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1040, 3);
            this.splitter1.TabIndex = 10;
            this.splitter1.TabStop = false;
            // 
            // tabControlCharts
            // 
            this.tabControlCharts.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControlCharts.Controls.Add(this.tabPage1);
            this.tabControlCharts.Controls.Add(this.tabPage2);
            this.tabControlCharts.Controls.Add(this.tabPage5);
            this.tabControlCharts.Controls.Add(this.tabPage4);
            this.tabControlCharts.Controls.Add(this.tabPage3);
            this.tabControlCharts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlCharts.Location = new System.Drawing.Point(0, 284);
            this.tabControlCharts.Name = "tabControlCharts";
            this.tabControlCharts.SelectedIndex = 0;
            this.tabControlCharts.Size = new System.Drawing.Size(1040, 268);
            this.tabControlCharts.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lineChartAverageSpeed);
            this.tabPage1.ImageIndex = 0;
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1032, 242);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Speed (Average)";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lineChartMomentSpeed);
            this.tabPage2.ImageIndex = 1;
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1032, 242);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Speed (Moment)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.lineChartAverageCPU);
            this.tabPage5.Location = new System.Drawing.Point(4, 4);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1032, 242);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "CPU (Average)";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lineChartAverageMemory);
            this.tabPage4.Location = new System.Drawing.Point(4, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1032, 242);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Memory (Average)";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lineChartAverageIO);
            this.tabPage3.Location = new System.Drawing.Point(4, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1032, 242);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "I/O (Average)";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lineChartAverageSpeed
            // 
            this.lineChartAverageSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartAverageSpeed.Location = new System.Drawing.Point(3, 3);
            this.lineChartAverageSpeed.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartAverageSpeed.Name = "lineChartAverageSpeed";
            this.lineChartAverageSpeed.Size = new System.Drawing.Size(1026, 236);
            this.lineChartAverageSpeed.TabIndex = 0;
            this.lineChartAverageSpeed.Title = "";
            // 
            // lineChartMomentSpeed
            // 
            this.lineChartMomentSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartMomentSpeed.Location = new System.Drawing.Point(3, 3);
            this.lineChartMomentSpeed.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartMomentSpeed.Name = "lineChartMomentSpeed";
            this.lineChartMomentSpeed.Size = new System.Drawing.Size(1026, 236);
            this.lineChartMomentSpeed.TabIndex = 0;
            this.lineChartMomentSpeed.Title = "";
            // 
            // lineChartAverageCPU
            // 
            this.lineChartAverageCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartAverageCPU.Location = new System.Drawing.Point(3, 3);
            this.lineChartAverageCPU.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartAverageCPU.Name = "lineChartAverageCPU";
            this.lineChartAverageCPU.Size = new System.Drawing.Size(1026, 236);
            this.lineChartAverageCPU.TabIndex = 0;
            this.lineChartAverageCPU.Title = "";
            // 
            // lineChartAverageMemory
            // 
            this.lineChartAverageMemory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartAverageMemory.Location = new System.Drawing.Point(3, 3);
            this.lineChartAverageMemory.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartAverageMemory.Name = "lineChartAverageMemory";
            this.lineChartAverageMemory.Size = new System.Drawing.Size(1026, 236);
            this.lineChartAverageMemory.TabIndex = 0;
            this.lineChartAverageMemory.Title = "";
            // 
            // lineChartAverageIO
            // 
            this.lineChartAverageIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartAverageIO.Location = new System.Drawing.Point(0, 0);
            this.lineChartAverageIO.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartAverageIO.Name = "lineChartAverageIO";
            this.lineChartAverageIO.Size = new System.Drawing.Size(1032, 242);
            this.lineChartAverageIO.TabIndex = 0;
            this.lineChartAverageIO.Title = "";
            // 
            // barChartSpeed
            // 
            this.barChartSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartSpeed.Location = new System.Drawing.Point(5, 5);
            this.barChartSpeed.MinimumSize = new System.Drawing.Size(1, 1);
            this.barChartSpeed.Name = "barChartSpeed";
            this.barChartSpeed.Size = new System.Drawing.Size(164, 271);
            this.barChartSpeed.TabIndex = 0;
            // 
            // barChartTime
            // 
            this.barChartTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartTime.Location = new System.Drawing.Point(177, 5);
            this.barChartTime.Name = "barChartTime";
            this.barChartTime.Size = new System.Drawing.Size(164, 271);
            this.barChartTime.TabIndex = 1;
            // 
            // barChartSize
            // 
            this.barChartSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartSize.Location = new System.Drawing.Point(349, 5);
            this.barChartSize.Name = "barChartSize";
            this.barChartSize.Size = new System.Drawing.Size(164, 271);
            this.barChartSize.TabIndex = 2;
            // 
            // barChartCPU
            // 
            this.barChartCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartCPU.Location = new System.Drawing.Point(521, 5);
            this.barChartCPU.Name = "barChartCPU";
            this.barChartCPU.Size = new System.Drawing.Size(164, 271);
            this.barChartCPU.TabIndex = 3;
            // 
            // barChartMemory
            // 
            this.barChartMemory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartMemory.Location = new System.Drawing.Point(693, 5);
            this.barChartMemory.Name = "barChartMemory";
            this.barChartMemory.Size = new System.Drawing.Size(167, 271);
            this.barChartMemory.TabIndex = 4;
            // 
            // barChartIO
            // 
            this.barChartIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartIO.Location = new System.Drawing.Point(868, 5);
            this.barChartIO.Name = "barChartIO";
            this.barChartIO.Size = new System.Drawing.Size(167, 271);
            this.barChartIO.TabIndex = 5;
            // 
            // StepFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 552);
            this.Controls.Add(this.tabControlCharts);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.LayoutPanel);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(16, 39);
            this.Name = "StepFrame";
            this.LayoutPanel.ResumeLayout(false);
            this.tabControlCharts.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.TableLayoutPanel LayoutPanel;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TabControl tabControlCharts;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public LineChartFrame lineChartAverageSpeed;
        public LineChartFrame lineChartMomentSpeed;
        private System.Windows.Forms.TabPage tabPage3;
        public LineChartFrame lineChartAverageIO;
        private System.Windows.Forms.TabPage tabPage4;
        public LineChartFrame lineChartAverageMemory;
        private System.Windows.Forms.TabPage tabPage5;
        public LineChartFrame lineChartAverageCPU;
        private BarChartFrame barChartSpeed;
        private BarChartFrame barChartTime;
        private BarChartFrame barChartSize;
        private BarChartFrame barChartCPU;
        private BarChartFrame barChartMemory;
        private BarChartFrame barChartIO;
    }
}
