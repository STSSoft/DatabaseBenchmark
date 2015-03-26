using DatabaseBenchmark.Frames;
using System;
using System.Windows.Forms;

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
            this.Text = ((Database)obj).Name;

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