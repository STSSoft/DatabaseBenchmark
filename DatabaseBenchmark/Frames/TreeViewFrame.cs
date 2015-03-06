using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using STS.General.GUI.Extensions;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class TreeViewFrame : DockContent
    {
        private BenchmarkInstanceProperies Properties;
        private ILog Logger;

        public TreeViewFrame()
        {
            InitializeComponent();

            Logger = LogManager.GetLogger("ApplicationLogger");
            treeView.ImageList = imageListTreeView;
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
                    Directory.Delete(directory, true);

                foreach (var database in databases)
                {
                    AddAfter(null, database);
                    database.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, database.DatabaseName);

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

            foreach (var item in treeView.Nodes.Iterate().Where(x => x.Tag != null && !((Database)x.Tag).DatabaseName.Equals(x.Text)))
            {
                item.BeginEdit();
                item.Name = ((Database)item.Tag).DatabaseName;
                item.Text = item.Name;
                item.EndEdit(true);
            }

            treeView.Update();
            Invalidate();

            this.ResumeLayout();
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
                var node1 = treeView.Nodes.BuildNode(newDatabase.Category, newDatabase.DatabaseName);
                node1.ImageIndex = 0;
                node1.Tag = newDatabase;
                node1.Checked = state;

                return;
            }

            TreeNode node = treeView.Nodes.Iterate().Where(x => x.Tag == database).FirstOrDefault();
            node.ImageIndex = 0;
            node.Checked = state;

            var nodes = node.Parent != null ? node.Parent.Nodes : treeView.Nodes;
            nodes.Insert(node.Index + 1, newDatabase.DatabaseName).Tag = newDatabase;
        }

        #region TreeView events

        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;

            foreach (var n in node.Nodes.Iterate())
                n.Checked = node.Checked;
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null && e.Node.Tag != null)
            {
                var renamedNode = e.Node;
                var newLabel = e.Label;
                var oldLabel = renamedNode.Text;

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

                ((Database)renamedNode.Tag).DatabaseName = newLabel;
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuDatabase.Items[0].Enabled = e.Node.Tag != null;
                contextMenuDatabase.Items[1].Enabled = e.Node.Tag != null;
                contextMenuDatabase.Items[6].Enabled = e.Node.Tag != null;

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
                var databaseType = selectedDatabase.GetType();
                var tempDatabase = (Database)Activator.CreateInstance(databaseType);

                tempDatabase.DatabaseName = treeView.SelectedNode.Text + " Clone";
                tempDatabase.DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, tempDatabase.DatabaseName);

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

            TreeNode selectedDatabase = treeView.Nodes.Iterate().Where(x => x.Name.Equals(treeView.SelectedNode.Name)).ToArray()[0];
            treeView.Nodes.Remove(selectedDatabase);

            treeView.Update();

            this.ResumeLayout();
        }

        private void restoreDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            CreateTreeView();

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