namespace DatabaseBenchmark.Frames
{
    partial class TreeViewFrame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeViewFrame));
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageListTreeView = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuDatabase = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.restoreDefaultAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separator3 = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxOrder = new System.Windows.Forms.GroupBox();
            this.TreeViewOrderComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tuningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuDatabase.SuspendLayout();
            this.groupBoxOrder.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.AllowDrop = true;
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.CheckBoxes = true;
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageListTreeView;
            this.treeView.LabelEdit = true;
            this.treeView.Location = new System.Drawing.Point(0, 36);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(241, 447);
            this.treeView.TabIndex = 1;
            this.treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
            this.treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterCheck);
            this.treeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            // 
            // imageListTreeView
            // 
            this.imageListTreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeView.ImageStream")));
            this.imageListTreeView.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTreeView.Images.SetKeyName(0, "Db-icon.png");
            // 
            // contextMenuDatabase
            // 
            this.contextMenuDatabase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cloneToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.separator1,
            this.restoreDefaultAllToolStripMenuItem,
            this.separator2,
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.separator3,
            this.tuningToolStripMenuItem,
            this.propertiesToolStripMenuItem});
            this.contextMenuDatabase.Name = "contextMenuDatabase";
            this.contextMenuDatabase.Size = new System.Drawing.Size(180, 284);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Image = global::DatabaseBenchmark.Properties.Resources.clone_24x24;
            this.cloneToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.cloneToolStripMenuItem.Text = "Clone";
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.cloneToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Image = global::DatabaseBenchmark.Properties.Resources.rename_24x24;
            this.renameToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::DatabaseBenchmark.Properties.Resources.delete_24x24;
            this.deleteToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            this.separator1.Size = new System.Drawing.Size(176, 6);
            // 
            // restoreDefaultAllToolStripMenuItem
            // 
            this.restoreDefaultAllToolStripMenuItem.Image = global::DatabaseBenchmark.Properties.Resources.default_24x24;
            this.restoreDefaultAllToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.restoreDefaultAllToolStripMenuItem.Name = "restoreDefaultAllToolStripMenuItem";
            this.restoreDefaultAllToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.restoreDefaultAllToolStripMenuItem.Text = "Restore Default All";
            this.restoreDefaultAllToolStripMenuItem.Click += new System.EventHandler(this.restoreDefaultAllToolStripMenuItem_Click);
            // 
            // separator2
            // 
            this.separator2.Name = "separator2";
            this.separator2.Size = new System.Drawing.Size(176, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.expandAllToolStripMenuItem.Text = "Expand All";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // separator3
            // 
            this.separator3.Name = "separator3";
            this.separator3.Size = new System.Drawing.Size(176, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Image = global::DatabaseBenchmark.Properties.Resources.options_24x24;
            this.propertiesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // groupBoxOrder
            // 
            this.groupBoxOrder.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxOrder.Controls.Add(this.TreeViewOrderComboBox);
            this.groupBoxOrder.Controls.Add(this.label1);
            this.groupBoxOrder.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxOrder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxOrder.Location = new System.Drawing.Point(0, 0);
            this.groupBoxOrder.Name = "groupBoxOrder";
            this.groupBoxOrder.Size = new System.Drawing.Size(241, 34);
            this.groupBoxOrder.TabIndex = 5;
            this.groupBoxOrder.TabStop = false;
            // 
            // TreeViewOrderComboBox
            // 
            this.TreeViewOrderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeViewOrderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TreeViewOrderComboBox.Items.AddRange(new object[] {
            "IndexTechnology",
            "Category"});
            this.TreeViewOrderComboBox.Location = new System.Drawing.Point(67, 9);
            this.TreeViewOrderComboBox.Name = "TreeViewOrderComboBox";
            this.TreeViewOrderComboBox.Size = new System.Drawing.Size(167, 21);
            this.TreeViewOrderComboBox.TabIndex = 3;
            this.TreeViewOrderComboBox.SelectedIndexChanged += new System.EventHandler(this.TreeViewOrderComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Order By:";
            // 
            // tuningToolStripMenuItem
            // 
            this.tuningToolStripMenuItem.Name = "tuningToolStripMenuItem";
            this.tuningToolStripMenuItem.Size = new System.Drawing.Size(179, 30);
            this.tuningToolStripMenuItem.Text = "Tuning";
            this.tuningToolStripMenuItem.Click += new System.EventHandler(this.tuningToolStripMenuItem_Click);
            // 
            // TreeViewFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 483);
            this.Controls.Add(this.groupBoxOrder);
            this.Controls.Add(this.treeView);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TreeViewFrame";
            this.ShowIcon = false;
            this.Text = "Databases";
            this.contextMenuDatabase.ResumeLayout(false);
            this.groupBoxOrder.ResumeLayout(false);
            this.groupBoxOrder.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuDatabase;
        private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator separator1;
        private System.Windows.Forms.ToolStripSeparator separator2;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ImageList imageListTreeView;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator separator3;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.GroupBox groupBoxOrder;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ComboBox TreeViewOrderComboBox;
        private System.Windows.Forms.ToolStripMenuItem tuningToolStripMenuItem;
    }
}