namespace DatabaseBenchmark.Report
{
    partial class ReportForm
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
            this.components = new System.ComponentModel.Container();
            this.groupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.lblModules = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxMemoryBanks = new System.Windows.Forms.TextBox();
            this.txtBoxMemoryFrequency = new System.Windows.Forms.TextBox();
            this.txtBoxMemoryType = new System.Windows.Forms.TextBox();
            this.txtBoxMemoryCapacity = new System.Windows.Forms.TextBox();
            this.txtBoxCpuCount = new System.Windows.Forms.TextBox();
            this.txtBoxCpuThreads = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBoxCpuFrequency = new System.Windows.Forms.TextBox();
            this.txtBoxCpuName = new System.Windows.Forms.TextBox();
            this.txtBoxAdditionalInfo = new System.Windows.Forms.TextBox();
            this.groupBoxStorage = new System.Windows.Forms.GroupBox();
            this.txtBoxHddSize = new System.Windows.Forms.TextBox();
            this.comboBoxStorageModel = new System.Windows.Forms.ComboBox();
            this.btnValidate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBoxOS = new System.Windows.Forms.GroupBox();
            this.txtBoxOsType = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtBoxOsName = new System.Windows.Forms.TextBox();
            this.txtBoxEmail = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtBoxDescription = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.cbUserDefined = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBoxShow = new System.Windows.Forms.CheckBox();
            this.groupBoxConfiguration.SuspendLayout();
            this.groupBoxStorage.SuspendLayout();
            this.groupBoxOS.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxConfiguration
            // 
            this.groupBoxConfiguration.Controls.Add(this.lblModules);
            this.groupBoxConfiguration.Controls.Add(this.label1);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxMemoryBanks);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxMemoryFrequency);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxMemoryType);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxMemoryCapacity);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxCpuCount);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxCpuThreads);
            this.groupBoxConfiguration.Controls.Add(this.label3);
            this.groupBoxConfiguration.Controls.Add(this.label2);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxCpuFrequency);
            this.groupBoxConfiguration.Controls.Add(this.txtBoxCpuName);
            this.groupBoxConfiguration.Location = new System.Drawing.Point(1, 146);
            this.groupBoxConfiguration.Name = "groupBoxConfiguration";
            this.groupBoxConfiguration.Size = new System.Drawing.Size(579, 90);
            this.groupBoxConfiguration.TabIndex = 0;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "CPU and Memory";
            // 
            // lblModules
            // 
            this.lblModules.AutoSize = true;
            this.lblModules.Location = new System.Drawing.Point(349, 58);
            this.lblModules.Name = "lblModules";
            this.lblModules.Size = new System.Drawing.Size(47, 13);
            this.lblModules.TabIndex = 21;
            this.lblModules.Text = "Modules";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "X";
            // 
            // txtBoxMemoryBanks
            // 
            this.txtBoxMemoryBanks.Location = new System.Drawing.Point(402, 54);
            this.txtBoxMemoryBanks.Name = "txtBoxMemoryBanks";
            this.txtBoxMemoryBanks.ReadOnly = true;
            this.txtBoxMemoryBanks.Size = new System.Drawing.Size(39, 20);
            this.txtBoxMemoryBanks.TabIndex = 15;
            // 
            // txtBoxMemoryFrequency
            // 
            this.txtBoxMemoryFrequency.Location = new System.Drawing.Point(508, 29);
            this.txtBoxMemoryFrequency.Name = "txtBoxMemoryFrequency";
            this.txtBoxMemoryFrequency.ReadOnly = true;
            this.txtBoxMemoryFrequency.Size = new System.Drawing.Size(63, 20);
            this.txtBoxMemoryFrequency.TabIndex = 11;
            // 
            // txtBoxMemoryType
            // 
            this.txtBoxMemoryType.Location = new System.Drawing.Point(447, 29);
            this.txtBoxMemoryType.Name = "txtBoxMemoryType";
            this.txtBoxMemoryType.ReadOnly = true;
            this.txtBoxMemoryType.Size = new System.Drawing.Size(55, 20);
            this.txtBoxMemoryType.TabIndex = 10;
            // 
            // txtBoxMemoryCapacity
            // 
            this.txtBoxMemoryCapacity.Location = new System.Drawing.Point(401, 29);
            this.txtBoxMemoryCapacity.Name = "txtBoxMemoryCapacity";
            this.txtBoxMemoryCapacity.ReadOnly = true;
            this.txtBoxMemoryCapacity.Size = new System.Drawing.Size(40, 20);
            this.txtBoxMemoryCapacity.TabIndex = 9;
            // 
            // txtBoxCpuCount
            // 
            this.txtBoxCpuCount.Location = new System.Drawing.Point(11, 29);
            this.txtBoxCpuCount.Name = "txtBoxCpuCount";
            this.txtBoxCpuCount.ReadOnly = true;
            this.txtBoxCpuCount.Size = new System.Drawing.Size(32, 20);
            this.txtBoxCpuCount.TabIndex = 7;
            // 
            // txtBoxCpuThreads
            // 
            this.txtBoxCpuThreads.Location = new System.Drawing.Point(255, 58);
            this.txtBoxCpuThreads.Name = "txtBoxCpuThreads";
            this.txtBoxCpuThreads.ReadOnly = true;
            this.txtBoxCpuThreads.Size = new System.Drawing.Size(55, 20);
            this.txtBoxCpuThreads.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(203, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Threads";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Max Clock";
            // 
            // txtBoxCpuFrequency
            // 
            this.txtBoxCpuFrequency.Location = new System.Drawing.Point(69, 55);
            this.txtBoxCpuFrequency.Name = "txtBoxCpuFrequency";
            this.txtBoxCpuFrequency.ReadOnly = true;
            this.txtBoxCpuFrequency.Size = new System.Drawing.Size(71, 20);
            this.txtBoxCpuFrequency.TabIndex = 1;
            // 
            // txtBoxCpuName
            // 
            this.txtBoxCpuName.Location = new System.Drawing.Point(69, 29);
            this.txtBoxCpuName.Name = "txtBoxCpuName";
            this.txtBoxCpuName.ReadOnly = true;
            this.txtBoxCpuName.Size = new System.Drawing.Size(241, 20);
            this.txtBoxCpuName.TabIndex = 0;
            // 
            // txtBoxAdditionalInfo
            // 
            this.txtBoxAdditionalInfo.Location = new System.Drawing.Point(6, 19);
            this.txtBoxAdditionalInfo.MaxLength = 250;
            this.txtBoxAdditionalInfo.Multiline = true;
            this.txtBoxAdditionalInfo.Name = "txtBoxAdditionalInfo";
            this.txtBoxAdditionalInfo.Size = new System.Drawing.Size(565, 41);
            this.txtBoxAdditionalInfo.TabIndex = 17;
            // 
            // groupBoxStorage
            // 
            this.groupBoxStorage.Controls.Add(this.txtBoxHddSize);
            this.groupBoxStorage.Controls.Add(this.comboBoxStorageModel);
            this.groupBoxStorage.Location = new System.Drawing.Point(323, 87);
            this.groupBoxStorage.Name = "groupBoxStorage";
            this.groupBoxStorage.Size = new System.Drawing.Size(257, 53);
            this.groupBoxStorage.TabIndex = 16;
            this.groupBoxStorage.TabStop = false;
            this.groupBoxStorage.Text = "Database Storage";
            // 
            // txtBoxHddSize
            // 
            this.txtBoxHddSize.Location = new System.Drawing.Point(186, 19);
            this.txtBoxHddSize.Name = "txtBoxHddSize";
            this.txtBoxHddSize.ReadOnly = true;
            this.txtBoxHddSize.Size = new System.Drawing.Size(63, 20);
            this.txtBoxHddSize.TabIndex = 18;
            // 
            // comboBoxStorageModel
            // 
            this.comboBoxStorageModel.FormattingEnabled = true;
            this.comboBoxStorageModel.Location = new System.Drawing.Point(6, 19);
            this.comboBoxStorageModel.Name = "comboBoxStorageModel";
            this.comboBoxStorageModel.Size = new System.Drawing.Size(174, 21);
            this.comboBoxStorageModel.TabIndex = 17;
            this.comboBoxStorageModel.SelectionChangeCommitted += new System.EventHandler(this.comboBoxStorageModel_SelectionChangeCommitted);
            // 
            // btnValidate
            // 
            this.btnValidate.Location = new System.Drawing.Point(424, 336);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(75, 23);
            this.btnValidate.TabIndex = 1;
            this.btnValidate.Text = "Send ";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(505, 336);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBoxOS
            // 
            this.groupBoxOS.Controls.Add(this.txtBoxOsType);
            this.groupBoxOS.Controls.Add(this.label11);
            this.groupBoxOS.Controls.Add(this.txtBoxOsName);
            this.groupBoxOS.Location = new System.Drawing.Point(1, 87);
            this.groupBoxOS.Name = "groupBoxOS";
            this.groupBoxOS.Size = new System.Drawing.Size(316, 53);
            this.groupBoxOS.TabIndex = 3;
            this.groupBoxOS.TabStop = false;
            this.groupBoxOS.Text = "Operating System";
            // 
            // txtBoxOsType
            // 
            this.txtBoxOsType.Location = new System.Drawing.Point(259, 20);
            this.txtBoxOsType.Name = "txtBoxOsType";
            this.txtBoxOsType.ReadOnly = true;
            this.txtBoxOsType.Size = new System.Drawing.Size(51, 20);
            this.txtBoxOsType.TabIndex = 2;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 22);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 1;
            this.label11.Text = "Name";
            // 
            // txtBoxOsName
            // 
            this.txtBoxOsName.Location = new System.Drawing.Point(47, 20);
            this.txtBoxOsName.Name = "txtBoxOsName";
            this.txtBoxOsName.ReadOnly = true;
            this.txtBoxOsName.Size = new System.Drawing.Size(206, 20);
            this.txtBoxOsName.TabIndex = 0;
            // 
            // txtBoxEmail
            // 
            this.txtBoxEmail.Location = new System.Drawing.Point(87, 314);
            this.txtBoxEmail.Name = "txtBoxEmail";
            this.txtBoxEmail.Size = new System.Drawing.Size(134, 20);
            this.txtBoxEmail.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(0, 317);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "E-mail (optional)";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtBoxAdditionalInfo);
            this.groupBox4.Location = new System.Drawing.Point(1, 242);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(579, 66);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Additional Information";
            // 
            // txtBoxDescription
            // 
            this.txtBoxDescription.Location = new System.Drawing.Point(7, 12);
            this.txtBoxDescription.Multiline = true;
            this.txtBoxDescription.Name = "txtBoxDescription";
            this.txtBoxDescription.ReadOnly = true;
            this.txtBoxDescription.Size = new System.Drawing.Size(241, 46);
            this.txtBoxDescription.TabIndex = 19;
            this.txtBoxDescription.Text = "The results validator sends the test result and computer configuration to our ded" +
    "icated servers.";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(259, 25);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(247, 13);
            this.linkLabel1.TabIndex = 20;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://stssoft.com/products/database-benchmark/";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(259, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Visit us on:";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(259, 64);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(297, 13);
            this.linkLabel2.TabIndex = 22;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "http://reporting.stssoft.com/management/public/benchmarks";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(259, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 13);
            this.label9.TabIndex = 23;
            this.label9.Text = "Other results:";
            // 
            // cbUserDefined
            // 
            this.cbUserDefined.AutoSize = true;
            this.cbUserDefined.Location = new System.Drawing.Point(7, 64);
            this.cbUserDefined.Name = "cbUserDefined";
            this.cbUserDefined.Size = new System.Drawing.Size(153, 17);
            this.cbUserDefined.TabIndex = 24;
            this.cbUserDefined.Text = "User Defined Configuration";
            this.cbUserDefined.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checkBoxShow
            // 
            this.checkBoxShow.AutoSize = true;
            this.checkBoxShow.Location = new System.Drawing.Point(3, 342);
            this.checkBoxShow.Name = "checkBoxShow";
            this.checkBoxShow.Size = new System.Drawing.Size(169, 17);
            this.checkBoxShow.TabIndex = 25;
            this.checkBoxShow.Text = "Don\'t show this window again.";
            this.checkBoxShow.UseVisualStyleBackColor = true;
            // 
            // ReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 367);
            this.Controls.Add(this.checkBoxShow);
            this.Controls.Add(this.cbUserDefined);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.txtBoxDescription);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtBoxEmail);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBoxOS);
            this.Controls.Add(this.groupBoxStorage);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Name = "ReportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Online Report";
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBoxConfiguration.PerformLayout();
            this.groupBoxStorage.ResumeLayout(false);
            this.groupBoxStorage.PerformLayout();
            this.groupBoxOS.ResumeLayout(false);
            this.groupBoxOS.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxConfiguration;
        private System.Windows.Forms.TextBox txtBoxCpuThreads;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBoxCpuFrequency;
        private System.Windows.Forms.TextBox txtBoxCpuName;
        private System.Windows.Forms.TextBox txtBoxMemoryCapacity;
        private System.Windows.Forms.TextBox txtBoxCpuCount;
        private System.Windows.Forms.TextBox txtBoxAdditionalInfo;
        private System.Windows.Forms.GroupBox groupBoxStorage;
        private System.Windows.Forms.ComboBox comboBoxStorageModel;
        private System.Windows.Forms.TextBox txtBoxMemoryBanks;
        private System.Windows.Forms.TextBox txtBoxMemoryFrequency;
        private System.Windows.Forms.TextBox txtBoxMemoryType;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBoxOS;
        private System.Windows.Forms.TextBox txtBoxOsType;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtBoxOsName;
        private System.Windows.Forms.TextBox txtBoxEmail;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtBoxDescription;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtBoxHddSize;
        private System.Windows.Forms.CheckBox cbUserDefined;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblModules;
        private System.Windows.Forms.CheckBox checkBoxShow;

    }
}