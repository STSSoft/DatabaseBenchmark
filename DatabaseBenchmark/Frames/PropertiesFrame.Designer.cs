namespace DatabaseBenchmark.Frames
{
    partial class PropertiesFrame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesFrame));
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.btnRestoreDefault = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(330, 364);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // btnRestoreDefault
            // 
            this.btnRestoreDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestoreDefault.Location = new System.Drawing.Point(176, 374);
            this.btnRestoreDefault.Name = "btnRestoreDefault";
            this.btnRestoreDefault.Size = new System.Drawing.Size(141, 23);
            this.btnRestoreDefault.TabIndex = 4;
            this.btnRestoreDefault.Text = "Restore Default";
            this.btnRestoreDefault.UseVisualStyleBackColor = true;
            this.btnRestoreDefault.Click += new System.EventHandler(this.restoreDefaultBtn_Click);
            // 
            // PropertiesFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 409);
            this.Controls.Add(this.btnRestoreDefault);
            this.Controls.Add(this.propertyGrid1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PropertiesFrame";
            this.ShowIcon = false;
            this.Text = "Properies";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button btnRestoreDefault;
    }
}