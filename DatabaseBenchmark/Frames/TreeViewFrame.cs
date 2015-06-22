using DatabaseBenchmark.Properties;
using DatabaseBenchmark.Serialization;
using log4net;
using STS.General.GUI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class TreeViewFrame : DockContent
    {
        private ILog Logger;

        public event Action<Object> SelectedDatabaseChanged; // Object = Database

        public bool TreeViewEnabled
        {
            get { return treeView.Enabled; }
            set 
            { 
                treeView.Enabled = value;
                groupBoxOrder.Enabled = value;  // TODO: Fix this later.
            }
        }

        public TreeViewFrame()
        {
            InitializeComponent();

            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);

        }

        public void CreateTreeView()
        {
            Type[] benchmarksTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Database)) && t.GetConstructor(new Type[] { }) != null).ToArray();
            Database[] databases = new Database[benchmarksTypes.Length];

            try
            {
                try
                {
                    for (int i = 0; i < benchmarksTypes.Length; i++)
                        databases[i] = (Database)Activator.CreateInstance(benchmarksTypes[i]);
                }
                catch (Exception exc)
                {
                    Logger.Error("Database create instance error...", exc);
                }

                if (!Directory.Exists(MainForm.DATABASES_DIRECTORY))
                    Directory.CreateDirectory(MainForm.DATABASES_DIRECTORY);

                foreach (var directory in Directory.GetDirectories(MainForm.DATABASES_DIRECTORY))
                {
                    try // Some databases hold the rights to their data directory.
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error("Database delete directory error...", exc);
                    }
                }

                foreach (var database in databases.OrderBy(db => db.Name))
                {
                    string treeViewType = radioBtn_Category.Checked ? database.Category : database.IndexingTechnology.ToString();

                    AddAfter(null, database, treeViewType);
                    database.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, database.Name);

                    if (!Directory.Exists(database.DataDirectory))
                        Directory.CreateDirectory(database.DataDirectory);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Databases initialization error...", exc);
            }

            treeView.ExpandAll();
            treeView.SelectedNode = treeView.Nodes[0];
        }

        public void CreateTreeViewNode(IDatabase database, bool state, bool order)
        {
            if (order)
            {
                AddAfter(null, database, database.Category, state);
                radioBtn_Category.Checked = true;
            }
            else
            {
                AddAfter(null, database, database.IndexingTechnology.ToString(), state);
                radioBtn_IndexTech.Checked = true;
            }

            if (!Directory.Exists(database.DataDirectory))
                Directory.CreateDirectory(database.DataDirectory);
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

        public Database[] GetSelectedBenchmarks()
        {
            return treeView.Nodes.Iterate().Where(x => x.Checked && x.Tag as Database != null).Select(y => y.Tag as Database).ToArray();
        }

        /// <summary>
        /// Returns all databases and their checked state.
        /// </summary>
        public Dictionary<IDatabase, bool> GetAllDatabases()
        {
            return treeView.Nodes.Iterate().Where(x => x.Tag != null).ToDictionary(x => x.Tag as IDatabase, v => v.Checked);
        }

        private void AddAfter(IDatabase database, IDatabase newDatabase, string treeViewType, bool state = false)
        {
            if (database == null)
            {
                TreeNode node1 = new TreeNode();

                node1 = treeView.Nodes.BuildNode(treeViewType, newDatabase.Name);

                node1.ImageIndex = 0;
                node1.Tag = newDatabase;
                node1.Checked = state;

                return;
            }

            TreeNode node = treeView.Nodes.Iterate().Where(x => x.Tag == database).FirstOrDefault();
            TreeNodeCollection nodes = node.Parent != null ? node.Parent.Nodes : treeView.Nodes;

            TreeNode newNode = nodes.Insert(node.Index + 1, newDatabase.Name);
            newNode.Tag = newDatabase;
            newNode.Checked = state;
            newNode.ImageIndex = 0;

            treeView.SelectedNode = newNode;

        }
        public bool IsSelectedBenchamrkNode
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
                //Database selectedDatabase = treeView.Nodes.Iterate().Where(x => x.Name.Equals(treeView.SelectedNode.Name)).Select(y => y.Tag as Database).ToArray()[0];
                var selectedDatabase = treeView.SelectedNode.Tag as Database;

                if (selectedDatabase != null)
                {
                    Type databaseType = selectedDatabase.GetType();
                    Database tempDatabase = (Database)Activator.CreateInstance(databaseType);

                    tempDatabase.Name = treeView.SelectedNode.Text + " Clone";
                    tempDatabase.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, tempDatabase.Name);

                    if (!Directory.Exists(tempDatabase.DataDirectory))
                        Directory.CreateDirectory(tempDatabase.DataDirectory);

                    string treeViewType = radioBtn_Category.Checked ? selectedDatabase.Category : selectedDatabase.IndexingTechnology.ToString();

                    AddAfter(selectedDatabase, tempDatabase, treeViewType);
                }

                treeView.Update();
            }
            catch (Exception exc)
            {
                Logger.Error("TreeView clone ...", exc);
            }
        }
        public void RenameNade()
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

                contextMenuDatabase.Items[0].Visible = state;
                contextMenuDatabase.Items[1].Visible = state;
                contextMenuDatabase.Items[8].Visible = state;
                contextMenuDatabase.Items[9].Visible = state;

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
            RenameNade();
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
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView.CollapseAll();
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
                DeleteNode();
            if (e.KeyData == Keys.Enter)
                SelectedDatabaseChanged.Invoke(treeView.SelectedNode.Tag);

        }

        #endregion

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

        public bool IsCategoryOrder()
        {
            return radioBtn_Category.Checked ? true : false;
        }

        private void radioBtn_IndexTech_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtn_IndexTech.Checked)
                SetIndexingOrder();
        }
        private void radioBtn_Category_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtn_Category.Checked)
                SetCategoryOrder();
        }
        public void SetCategoryOrder()
        {
            List<TreeNode> newTree = new List<TreeNode>();
            newTree = treeView.Nodes.Iterate().Where(x => x.Tag != null).OrderBy(db => db.Name).ToList();

            ClearTreeViewNodes();

            foreach (var node in newTree)
            {
                TreeNode node1 = new TreeNode();
                node1 = treeView.Nodes.BuildNode((node.Tag as Database).Category, (node.Tag as Database).Name);

                node1.ImageIndex = 0;
                node1.Tag = node.Tag;
                node1.Checked = node.Checked;
            }
            SetTreeView();        
        }

        public void SetIndexingOrder()
        {

            var newtree = treeView.Nodes.Iterate().Where(x => x.Tag != null)
                         .GroupBy(db => ((Database)db.Tag).IndexingTechnology)
                         .ToList();

            ClearTreeViewNodes();

            foreach (var node in newtree)
            {
                var t = node.ToArray();
                TreeNode newNode = new TreeNode(node.Key.ToString(), t);
                treeView.Nodes.Add(newNode);
            }
            SetTreeView();   
        }

        private void SetTreeView()
        {
           if (SelectedDatabaseChanged != null)
               SelectedDatabaseChanged.Invoke(null);

            SelectFirstNode();

            treeView.ExpandAll();
            treeView.Focus();
        }
    }
}