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
            this.barChartSpeed = new DatabaseBenchmark.Charts.BarChart();
            this.barChartTime = new DatabaseBenchmark.Charts.BarChart();
            this.barChartCPU = new DatabaseBenchmark.Charts.BarChart();
            this.barChartIO = new DatabaseBenchmark.Charts.BarChart();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tabControlCharts = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lineChartAverageSpeed = new DatabaseBenchmark.Charts.LineChart();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lineChartMomentSpeed = new DatabaseBenchmark.Charts.LineChart();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lineChartMomentMemory = new DatabaseBenchmark.Charts.LineChart();
            this.barChartMemory = new DatabaseBenchmark.Charts.BarChart();
            this.barChartSize = new DatabaseBenchmark.Charts.BarChart();
            this.LayoutPanel.SuspendLayout();
            this.tabControlCharts.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage4.SuspendLayout();
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
            // barChartCPU
            // 
            this.barChartCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartCPU.Location = new System.Drawing.Point(521, 5);
            this.barChartCPU.Name = "barChartCPU";
            this.barChartCPU.Size = new System.Drawing.Size(164, 271);
            this.barChartCPU.TabIndex = 3;
            this.barChartCPU.Visible = false;
            // 
            // barChartIO
            // 
            this.barChartIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartIO.Location = new System.Drawing.Point(868, 5);
            this.barChartIO.Name = "barChartIO";
            this.barChartIO.Size = new System.Drawing.Size(167, 271);
            this.barChartIO.TabIndex = 5;
            this.barChartIO.Visible = false;
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
            this.tabControlCharts.Controls.Add(this.tabPage4);
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
            // lineChartAverageSpeed
            // 
            this.lineChartAverageSpeed.AxisXTitle = "";
            this.lineChartAverageSpeed.AxisYTitle = "";
            this.lineChartAverageSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartAverageSpeed.LegendVisible = true;
            this.lineChartAverageSpeed.Location = new System.Drawing.Point(3, 3);
            this.lineChartAverageSpeed.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartAverageSpeed.Name = "lineChartAverageSpeed";
            this.lineChartAverageSpeed.Size = new System.Drawing.Size(1026, 236);
            this.lineChartAverageSpeed.TabIndex = 0;
            this.lineChartAverageSpeed.Title = "";
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
            // lineChartMomentSpeed
            // 
            this.lineChartMomentSpeed.AxisXTitle = "";
            this.lineChartMomentSpeed.AxisYTitle = "";
            this.lineChartMomentSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartMomentSpeed.LegendVisible = true;
            this.lineChartMomentSpeed.Location = new System.Drawing.Point(3, 3);
            this.lineChartMomentSpeed.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartMomentSpeed.Name = "lineChartMomentSpeed";
            this.lineChartMomentSpeed.Size = new System.Drawing.Size(1026, 236);
            this.lineChartMomentSpeed.TabIndex = 0;
            this.lineChartMomentSpeed.Title = "";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lineChartMomentMemory);
            this.tabPage4.Location = new System.Drawing.Point(4, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1032, 242);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Memory (Moment)";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // lineChartMomentMemory
            // 
            this.lineChartMomentMemory.AxisXTitle = "";
            this.lineChartMomentMemory.AxisYTitle = "";
            this.lineChartMomentMemory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineChartMomentMemory.LegendVisible = true;
            this.lineChartMomentMemory.Location = new System.Drawing.Point(3, 3);
            this.lineChartMomentMemory.MinimumSize = new System.Drawing.Size(1, 1);
            this.lineChartMomentMemory.Name = "lineChartMomentMemory";
            this.lineChartMomentMemory.Size = new System.Drawing.Size(1026, 236);
            this.lineChartMomentMemory.TabIndex = 0;
            this.lineChartMomentMemory.Title = "";
            // 
            // barChartMemory
            // 
            this.barChartMemory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartMemory.Location = new System.Drawing.Point(693, 5);
            this.barChartMemory.Name = "barChartMemory";
            this.barChartMemory.Size = new System.Drawing.Size(167, 271);
            this.barChartMemory.TabIndex = 4;
            // 
            // barChartSize
            // 
            this.barChartSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChartSize.Location = new System.Drawing.Point(349, 5);
            this.barChartSize.Name = "barChartSize";
            this.barChartSize.Size = new System.Drawing.Size(164, 271);
            this.barChartSize.TabIndex = 2;
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
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.HideOnClose = true;
            this.MinimumSize = new System.Drawing.Size(16, 39);
            this.Name = "StepFrame";
            this.ShowIcon = false;
            this.LayoutPanel.ResumeLayout(false);
            this.tabControlCharts.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
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
        public LineChart lineChartAverageSpeed;
        public LineChart lineChartMomentSpeed;
        private System.Windows.Forms.TabPage tabPage4;
        public LineChart lineChartMomentMemory;
        private BarChart barChartSpeed;
        private BarChart barChartTime;
        private BarChart barChartCPU;
        private BarChart barChartIO;
        private BarChart barChartSize;
        private BarChart barChartMemory;
    }
}
