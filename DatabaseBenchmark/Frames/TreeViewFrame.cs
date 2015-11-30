using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Utils;
using DatabaseBenchmark.Properties;
using DatabaseBenchmark.Utils;
using log4net;
using STS.General.GUI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class TreeViewFrame : DockContent
    {
        private ILog Logger;
        private List<Database> TuningDatabaseInstances;

        public TreeViewOrder treeViewOrder;
        public event Action<Object> SelectedDatabaseChanged; // Object = Database

        public TreeViewFrame()
        {
            InitializeComponent();

            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
            TreeViewOrderComboBox.Text = TreeViewOrder.Category.ToString();
        }

        public void CreateTreeView()
        {
            try
            {
                Database[] databases = ReflectionUtils.CreateDatabaseInstances();
                DirectoryUtils.CreateAndSetDatabasesDataDirectory(MainForm.DATABASES_DIRECTORY, databases);

                foreach (var database in databases.OrderBy(db => db.Name))
                    CreateTreeViewNode(database, false);

                treeView.ExpandAll();

                var node = treeView.Nodes[0];
                treeView.SelectedNode = node;

                TreeViewOrderComboBox.Text = treeViewOrder.ToString();

                if (SelectedDatabaseChanged != null)
                    SelectedDatabaseChanged.Invoke(node.Tag);

            }
            catch (Exception exc)
            {
                Logger.Error("TreeView creation error...", exc);
            }
        }

        public void CreateTreeViewNode(IDatabase database, bool state)
        {
            TreeNode node1 = new TreeNode();
            node1 = CreateTreeNode(database);
            SetTreeNodeProperies(node1, database, 0, state);
        }

        public void RefreshTreeView()
        {
            this.SuspendLayout();

            foreach (var item in treeView.Nodes.Iterate().Where(x => x.Tag != null && !((Database)x.Tag).Name.Equals(x.Text)))
            {
                item.BeginEdit();
                item.Name = ((Database)item.Tag).Name;
                item.Text = item.Name;
                item.EndEdit(true);
            }

            treeView.Update();
            Invalidate();

            this.ResumeLayout();
        }

        public void ClearTreeViewNodes()
        {
            treeView.Nodes.Clear();
        }

        public void ExpandAll()
        {
            treeView.ExpandAll();
        }

        public void CollapseAll()
        {
            treeView.CollapseAll();
        }

        public void SelectFirstNode()
        {
            treeView.SelectedNode = treeView.Nodes[0];
        }

        public Database[] GetSelectedDatabases()
        {
            if (TuningDatabaseInstances != null)
                return TuningDatabaseInstances.ToArray();

            return treeView.Nodes.Iterate().Where(x => x.Checked && x.Tag as Database != null).Select(y => y.Tag as Database).ToArray();
        }

        /// <summary>
        /// Returns all databases and their checked state.
        /// </summary>
        public Dictionary<IDatabase, bool> GetAllDatabasesAndCheckedStates()
        {
            return treeView.Nodes.Iterate().Where(x => x.Tag != null).ToDictionary(x => x.Tag as IDatabase, v => v.Checked);
        }

        public bool TreeViewEnabled
        {
            get { return treeView.Enabled; }
            set
            {
                treeView.Enabled = value;
                groupBoxOrder.Enabled = value;
            }
        }

        public List<Database> GetTuningDatabaseInstances()
        {
            return TuningDatabaseInstances;
        }

        public bool IsSelectedNodeDatabase
        {
            get
            {
                if (treeView.SelectedNode == null)
                    return false;

                return treeView.SelectedNode.Tag != null;
            }
        }

        public void CloneNode()
        {
            if (treeView.SelectedNode == null)
                return;

            try
            {
                var selectedDatabase = treeView.SelectedNode.Tag as Database;

                if (selectedDatabase != null)
                {
                    Type databaseType = selectedDatabase.GetType();

                    Database tempDatabase = ReflectionUtils.CreateDatabaseInstance(databaseType);
                    tempDatabase.Name = treeView.SelectedNode.Text + " Clone";

                    DirectoryUtils.CreateAndSetDatabaseDirectory(MainForm.DATABASES_DIRECTORY, tempDatabase);

                    TreeNode node = treeView.Nodes.Iterate().Where(x => x.Tag == selectedDatabase).FirstOrDefault();
                    TreeNodeCollection nodes = node.Parent != null ? node.Parent.Nodes : treeView.Nodes;

                    TreeNode newNode = nodes.Insert(node.Index + 1, tempDatabase.Name);
                    SetTreeNodeProperies(newNode, tempDatabase, 0, false);

                    treeView.SelectedNode = newNode;
                }

                treeView.Update();
            }
            catch (Exception exc)
            {
                Logger.Error("TreeView clone ...", exc);
            }
        }

        public void RenameNode()
        {
            if (treeView.SelectedNode == null)
                return;

            if (treeView.SelectedNode.Tag != null)
                treeView.SelectedNode.BeginEdit();
        }

        public void DeleteNode()
        {
            if (treeView.SelectedNode == null)
                return;

            treeView.Nodes.Remove(treeView.SelectedNode);
            treeView.Update();
        }

        public void RestoreDefault()
        {
            if (treeView.SelectedNode == null)
                return;

            try
            {
                TreeNode selectedNode = treeView.SelectedNode;
                Database instance = Activator.CreateInstance(selectedNode.Tag.GetType()) as Database;
                instance.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, instance.Name);

                selectedNode.Tag = instance;
                selectedNode.Text = instance.Name;

                if (SelectedDatabaseChanged != null)
                    SelectedDatabaseChanged.Invoke(instance);

            }
            catch (Exception exc)
            {
                Logger.Error("TreeView restore ...", exc);
            }
        }

        public Database GetSelectedDatabase()
        {
            return treeView.SelectedNode.Tag as Database;
        }

        public void SetTreeViewOrder()
        {
            var newTree = treeView.Nodes.Iterate().Where(x => x.Tag != null).OrderBy(db => db.Name);

            List<KeyValuePair<string, TreeNode>> groupedTree;

            if (treeViewOrder == TreeViewOrder.Category)
                groupedTree = newTree.Select(x => new KeyValuePair<string, TreeNode>((x.Tag as Database).Category, x)).ToList();
            else
                groupedTree = newTree.Select(x => new KeyValuePair<string, TreeNode>((x.Tag as Database).IndexingTechnology.ToString(), x)).ToList();

            ClearTreeViewNodes();

            foreach (var item in groupedTree.OrderBy(node => node.Key))
            {
                Database database = item.Value.Tag as Database;

                TreeNode node1 = CreateTreeNode(database);
                SetTreeNodeProperies(node1, database, 0, item.Value.Checked);
            }

            if (SelectedDatabaseChanged != null)
                SelectedDatabaseChanged.Invoke(null);

            SelectFirstNode();

            treeView.ExpandAll();
            treeView.Focus();
        }

        private TreeNode CreateTreeNode(IDatabase database)
        {
            TreeNode node = new TreeNode();
            TreeViewOrderComboBox.Text = treeViewOrder.ToString();


            if (treeViewOrder == TreeViewOrder.Category)
                node = treeView.Nodes.BuildNode(database.Category, database.Name);
            else
                node = treeView.Nodes.BuildNode(database.IndexingTechnology, database.Name);

            return node;
        }

        private void SetTreeNodeProperies(TreeNode node, IDatabase database, int imageIndex, bool state)
        {
            node.Tag = database;
            node.ImageIndex = imageIndex;
            node.Checked = state;
        }

        #region TreeView events

        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            foreach (var n in node.Nodes.Iterate())
                n.Checked = node.Checked;
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null && e.Node.Tag != null)
            {
                TreeNode renamedNode = e.Node;
                string newLabel = e.Label;
                string oldLabel = renamedNode.Text;

                foreach (var item in treeView.Nodes.Iterate().Where(x => x.Tag != null))
                {
                    if (newLabel.Equals(item.Text))
                    {
                        MessageBox.Show("Dublicate name exists.");
                        treeView.BeginInvoke(new Action(() => renamedNode.Text = oldLabel));

                        return;
                    }
                }

                renamedNode.Text = newLabel;
                renamedNode.Name = newLabel;

                ((Database)renamedNode.Tag).Name = newLabel;
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView.SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right)
            {
                bool state = e.Node.Tag != null;

                contextMenuDatabase.Items["cloneToolStripMenuItem"].Visible = state;
                contextMenuDatabase.Items["renameToolStripMenuItem"].Visible = state;
                contextMenuDatabase.Items["separator3"].Visible = state;
                contextMenuDatabase.Items["tuningToolStripMenuItem"].Visible = state;
                contextMenuDatabase.Items["propertiesToolStripMenuItem"].Visible = state;

                contextMenuDatabase.Show(MousePosition);
            }

            if (SelectedDatabaseChanged != null)
                SelectedDatabaseChanged.Invoke(e.Node.Tag);
        }

        #endregion

        #region Databases ContextMenu

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            CloneNode();
            this.ResumeLayout();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameNode();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            DeleteNode();
            this.ResumeLayout();
        }

        private void restoreDefaultAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            ClearTreeViewNodes();
            CreateTreeView();
            this.ResumeLayout();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CollapseAll();
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
                DeleteNode();
            if (e.KeyData == Keys.Enter)
                SelectedDatabaseChanged.Invoke(treeView.SelectedNode.Tag);
        }

        #endregion

        private void TreeViewOrderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeViewOrder = (TreeViewOrder)Enum.Parse(typeof(TreeViewOrder), TreeViewOrderComboBox.Text);
            try
            {
                SetTreeViewOrder(); //TODO: fix 
            }
            catch
            { }
        }

        public event EventHandler PropertiesClick
        {
            add
            {
                propertiesToolStripMenuItem.Click += value;
            }

            remove
            {
                propertiesToolStripMenuItem.Click -= value;
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tuningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TuningForm tuning = new TuningForm();
            tuning.Initialize(ReflectionUtils.GetPublicPropertiesAndValues(GetSelectedDatabase()), GetSelectedDatabase());
            tuning.Name = GetSelectedDatabase().Name;

            tuning.ShowDialog();

            if (tuning.DialogResult == DialogResult.OK)
            {
                TuningDatabaseInstances = new List<Database>();
                TuningDatabaseInstances = tuning.GetTuningDatabaseInstances();

                tuning.Close();
            }
        }

        public event EventHandler DatabaseClick
        {
            add 
            {
                treeView.Click += value;
            }
            remove
            {
                treeView.Click -= value;
            }
        }
    }

    public enum TreeViewOrder
    {
        Category,
        IndexTechnology
    }
}