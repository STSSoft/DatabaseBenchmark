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
        public TreeViewFrame Caller { get; set; }
        private PropertyDescriptor CurrentHedenProperty;
        private PropertyGridUtils descriptor;

        public PropertiesFrame()
        {
            InitializeComponent();       
        }

        public void SetProperties(Object obj)
        {
            propertyGrid1.SelectedObject = obj;

            if(obj!=null)
            descriptor = new PropertyGridUtils(obj.GetType());

            Invalidate();
        }

        public void HideProperty(string property, object database)
        {
            PropertyDescriptor currentproperty = TypeDescriptor.GetProperties(database)[property];

            descriptor.RemoveProperty(property);

            if (CurrentHedenProperty != null)
                descriptor.AddProperty(CurrentHedenProperty);

            propertyGrid1.SelectedObject = descriptor.FromComponent(database);
            CurrentHedenProperty = currentproperty;
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

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Name")
                Caller.RefreshTreeView();
        }

        private void restoreDefaultBtn_Click(object sender, EventArgs e)
        {
            Caller.RestoreDefault();
            propertyGrid1.SelectedObject = Caller.GetSelectedDatabase();
        }
    }
}