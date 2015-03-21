using DatabaseBenchmark.Benchmarking;
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
    public class LayoutManager
    {
        private int Count;

        public TreeViewFrame TreeView { get; private set; }
        public DockPanel Panel { get; private set; }
        public LogFrame LogFrame { get; private set; }

        public Dictionary<TestMethod, StepFrame> Frames { get; private set; }
        public ToolStripComboBox[] ComboBoxes { get; private set; }
        public TrackBar TrackBar { get; private set; }

        public LayoutManager(DockPanel panel, ToolStripComboBox[] comboBoxes, TrackBar trackBar)
        {
            Panel = panel;
            ComboBoxes = comboBoxes;
            TrackBar = trackBar;

            TreeView = new TreeViewFrame();
            Frames = new Dictionary<TestMethod, StepFrame>();
            LogFrame = new LogFrame();

            Initialize();

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
                Frames[method] = CreateStepFrame(method);
        }

        public void Initialize()
        {
            TrackBar.Value = 20;
            ComboBoxes[0].SelectedIndex = 0;
            ComboBoxes[1].SelectedIndex = 5;
        }

        public void Reset()
        {
            TreeView.Dispose();

            CreateTreeView();

            foreach (var item in Frames)
            {
                item.Value.Show(Panel);
                item.Value.DockState = DockState.Document;
            }

            Frames[TestMethod.Write].Activate();

            LogFrame.Dispose();
            LogFrame = new LogFrame();

            LogFrame.Show(Panel);
            LogFrame.DockState = DockState.DockBottomAutoHide;
            LogFrame.Text = "Logs";

            Initialize();
        }

        public void StoreDocking(string dockConfigPath)
        {
            Panel.SaveAsXml(dockConfigPath);
        }

        public void LoadDocking(string dockConfigPath)
        {
            Panel.LoadFromXml(dockConfigPath, new DeserializeDockContent(GetContentFromPersistString));

            TreeView.Text = "Databases";
            LogFrame.Text = "Logs";
        }

        public void SelectFrame(TestMethod method)
        {
            StepFrame frame = Frames[method];
            frame.Show(Panel);
        }

        public void SelectTreeView()
        {
            if (TreeView.IsDisposed)
            {
                CreateTreeView();

                return;
            }

            if (TreeView.IsHidden)
                TreeView.Show(Panel);
            else
                TreeView.Show(Panel);
        }

        public Dictionary<string, string> GetSelectedFromComboBoxes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var comboBox in ComboBoxes)
                result.Add(comboBox.Name, comboBox.Text);

            return result;
        }

        public void ShowBarChart(int column, bool visible)
        {
            foreach (var item in Frames)
            {
                StepFrame frame = item.Value;

                if (visible)
                    frame.LayoutPanel.ColumnStyles[column] = new ColumnStyle(SizeType.Percent, 18);
                else
                    frame.LayoutPanel.ColumnStyles[column] = new ColumnStyle(SizeType.Absolute, 0);
            }
        }

        public void SetLogarithmic(bool isChecked)
        {
            foreach (var item in Frames)
                item.Value.SetLogarithmic(isChecked);
        }

        # region Private members

        private void CreateTreeView()
        {
            TreeView = new TreeViewFrame();
            TreeView.CreateTreeView();
            TreeView.Text = "Databases";

            TreeView.Show(Panel);
            TreeView.DockState = DockState.DockLeft;
            TreeView.ExpandAll();
        }

        private StepFrame CreateStepFrame(TestMethod method)
        {
            StepFrame stepFrame = new StepFrame();
            stepFrame.Text = method.ToString();
            stepFrame.Dock = DockStyle.Fill;

            // Hide time, CPU, memory and I/O view from the layout.
            stepFrame.LayoutPanel.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 0);
            stepFrame.LayoutPanel.ColumnStyles[3] = new ColumnStyle(SizeType.Absolute, 0);
            stepFrame.LayoutPanel.ColumnStyles[4] = new ColumnStyle(SizeType.Absolute, 0);
            stepFrame.LayoutPanel.ColumnStyles[5] = new ColumnStyle(SizeType.Absolute, 0);

            return stepFrame;
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return TreeView;

            if (persistString == typeof(LogFrame).ToString())
                return LogFrame;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if (Count == 0)
                    frame = Frames[TestMethod.Write];
                else if (Count == 1)
                    frame = Frames[TestMethod.Read];
                else if (Count == 2)
                    frame = Frames[TestMethod.SecondaryRead];

                Count++;
            }

            return frame;
        }
        #endregion

    }
}
