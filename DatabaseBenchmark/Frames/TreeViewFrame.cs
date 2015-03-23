using DatabaseBenchmark.Properties;
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
        private BenchmarkInstanceProperies Properties;
        private ILog Logger;

        public bool TreeViewEnabled
        {
            get { return treeView.Enabled; }
            set { treeView.Enabled = value; }
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
                for (int i = 0; i < benchmarksTypes.Length; i++)
                    databases[i] = (Database)Activator.CreateInstance(benchmarksTypes[i]);

                if (!Directory.Exists(MainForm.DATABASES_DIRECTORY))
                    Directory.CreateDirectory(MainForm.DATABASES_DIRECTORY);

                foreach (var directory in Directory.GetDirectories(MainForm.DATABASES_DIRECTORY))
                {
                    try // Some databases hold rights to directory
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Database delete directory error...", e);
                    }
                }

                foreach (var database in databases)
                {
                    AddAfter(null, database);
                    database.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, database.Name);

                    if (!Directory.Exists(database.DataDirectory))
                        Directory.CreateDirectory(database.DataDirectory);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Database initialization error...", exc);
            }

            treeView.ExpandAll();
        }

        public void CreateTreeViewNode(IDatabase database, bool state)
        {
            AddAfter(null, database, state);

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

        private void AddAfter(IDatabase database, IDatabase newDatabase, bool state = false)
        {
            if (database == null)
            {
                var node1 = treeView.Nodes.BuildNode(newDatabase.Category, newDatabase.Name);
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

                contextMenuDatabase.Items[0].Enabled = state;
                contextMenuDatabase.Items[1].Enabled = state;
                contextMenuDatabase.Items[4].Enabled = state;
                contextMenuDatabase.Items[7].Enabled = state;

                contextMenuDatabase.Show(MousePosition);
            }
        }

        #endregion

        #region Databases ContextMenu

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            Database selectedDatabase = treeView.Nodes.Iterate().Where(x => x.Name.Equals(treeView.SelectedNode.Name)).Select(y => y.Tag as Database).ToArray()[0];

            if (selectedDatabase != null)
            {
                Type databaseType = selectedDatabase.GetType();
                Database tempDatabase = (Database)Activator.CreateInstance(databaseType);

                tempDatabase.Name = treeView.SelectedNode.Text + " Clone";
                tempDatabase.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, tempDatabase.Name);

                if (!Directory.Exists(tempDatabase.DataDirectory))
                    Directory.CreateDirectory(tempDatabase.DataDirectory);

                AddAfter(selectedDatabase, tempDatabase);
            }

            treeView.Update();

            this.ResumeLayout();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode.Tag != null)
                treeView.SelectedNode.BeginEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            treeView.Nodes.Remove(treeView.SelectedNode);
            treeView.Update();

            this.ResumeLayout();
        }

        private void restoreDefaultAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            CreateTreeView();
            this.ResumeLayout();
        }

        private void restoreDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            TreeNode selectedNode = treeView.SelectedNode;
            TreeNode prevNode = selectedNode.PrevNode;
            IDatabase instance = Activator.CreateInstance(selectedNode.Tag.GetType()) as IDatabase;

            deleteToolStripMenuItem_Click(sender, e);
            AddAfter(prevNode.Tag as IDatabase, instance, selectedNode.Checked);
            treeView.SelectedNode = treeView.Nodes.Iterate().First(x => x.Tag == instance);

            this.ResumeLayout();
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedDatabase = treeView.Nodes.Iterate().Where(x => x.Name.Equals(treeView.SelectedNode.Name)).Select(y => y.Tag as Database).ToArray()[0];

            if (selectedDatabase != null)
            {
                if (Properties != null)
                    Properties.Dispose();

                Properties = new BenchmarkInstanceProperies();

                Properties.Caller = this;
                Properties.Visible = true;
                Properties.SetProperties(selectedDatabase);
            }
        }

        #endregion
    }
}