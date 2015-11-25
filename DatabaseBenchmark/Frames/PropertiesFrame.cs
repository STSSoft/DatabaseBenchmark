using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Utils;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class PropertiesFrame : DockContent
    {
        public Form Caller { get; set; }

        public PropertiesFrame()
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
                Caller.Refresh();
                this.Close();

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Name")
                Caller.Refresh();
        }

        private void restoreDefaultBtn_Click(object sender, EventArgs e)
        {
        }
    }
}