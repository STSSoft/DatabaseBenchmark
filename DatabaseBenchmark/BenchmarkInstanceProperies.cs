using DatabaseBenchmark.Frames;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark
{
    public partial class BenchmarkInstanceProperies : DockContent
    {
        public TreeViewFrame Caller { get; set; }

        public BenchmarkInstanceProperies()
        {
            InitializeComponent();
        }

        public void SetProperties(Object obj)
        {
            propertyGrid1.SelectedObject = obj;
            Invalidate();
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

        private void restoreDefaultBtn_Click(object sender, EventArgs e)
        {
            Caller.RestoreDefault();
            propertyGrid1.SelectedObject = Caller.GetSelectedDatabase();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Name")
                Caller.RefreshTreeView();
        }
    }
}