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

        public event Action<string> DatabaseNameChanged;

        public PropertiesFrame()
        {
            InitializeComponent();       
        }

        public void SetProperties(Object obj)
        {
            if (propertyGrid1.IsDisposed)
                return;

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
            GridItem changedItem = e.ChangedItem;
            if (changedItem.Label == "Name")
            {
                //Caller.Refresh();
                if (DatabaseNameChanged != null)
                    DatabaseNameChanged.Invoke(changedItem.Value.ToString());
            }         
        }

        private void restoreDefaultBtn_Click(object sender, EventArgs e)
        {
            var test = Caller;
        }
    }
}