using DatabaseBenchmark.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Serialization
{
    public class AppSettings
    {
        public DockPanel DockingPanel { get; set; }
        public TreeViewFrame TreeView { get; set; }
        public ToolStripComboBox[] ComboBoxes { get; set; }
        public TrackBar TrackBar { get; set; }

        public Dictionary<string, StepFrame> Frames { get; private set; }

        public AppSettings(DockPanel panel, TreeViewFrame treeView, ToolStripComboBox[] comboBoxes, TrackBar trackBar)
        {
            DockingPanel = panel;
            TreeView = treeView;
            ComboBoxes = comboBoxes;
            TrackBar = trackBar;

            Frames = new Dictionary<string, StepFrame>();
        }
    }
}
