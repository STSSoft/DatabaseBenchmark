namespace DatabaseBenchmark.Frames
{
    partial class LogFrame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogFrame));
            this.textBoxTestLogs = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxTestLogs
            // 
            this.textBoxTestLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTestLogs.Location = new System.Drawing.Point(0, 0);
            this.textBoxTestLogs.Multiline = true;
            this.textBoxTestLogs.Name = "textBoxTestLogs";
            this.textBoxTestLogs.ReadOnly = true;
            this.textBoxTestLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxTestLogs.Size = new System.Drawing.Size(871, 98);
            this.textBoxTestLogs.TabIndex = 1;
            // 
            // LogFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 98);
            this.Controls.Add(this.textBoxTestLogs);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogFrame";
            this.Text = "LogFrame";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTestLogs;

    }
}