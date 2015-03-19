using DatabaseBenchmark.Frames;
using System.Collections.Generic;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Serialization
{
    public class ProjectSettings
    {
        public DockPanel DockingPanel { get; set; }
        public TreeViewFrame TreeView { get; set; }
        public ToolStripComboBox[] ComboBoxes { get; set; }
        public TrackBar TrackBar { get; set; }
        public LogFrame LogFrame { get; set; }

        public Dictionary<string, StepFrame> Frames { get; private set; }

        public ProjectSettings(DockPanel panel, TreeViewFrame treeView, ToolStripComboBox[] comboBoxes, TrackBar trackBar)
        {
            DockingPanel = panel;
            TreeView = treeView;
            ComboBoxes = comboBoxes;
            TrackBar = trackBar;

            LogFrame = new LogFrame();
            Frames = new Dictionary<string, StepFrame>();
        }
    }
}
