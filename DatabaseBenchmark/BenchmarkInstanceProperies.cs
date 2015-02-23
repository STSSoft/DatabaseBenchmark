using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseBenchmark.Frames;

namespace DatabaseBenchmark
{
    public partial class BenchmarkInstanceProperies : Form
    {
        public TreeViewFrame Caller { get; set; }

        public BenchmarkInstanceProperies()
        {
            InitializeComponent();
            CenterToScreen();
        }

        public void SetProperties(Object obj)
        {
            propertyGrid1.SelectedObject = obj;
            this.Text = ((Database)obj).DatabaseName;

            Invalidate();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
            Caller.RefreshTreeView();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                Caller.RefreshTreeView();

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}