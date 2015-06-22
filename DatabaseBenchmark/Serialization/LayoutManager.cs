using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Frames;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public BenchmarkInstanceProperies InstanceProperites { get; private set; }
        public Dictionary<TestMethod, StepFrame> StepFrames { get; private set; }
        public ToolStripComboBox[] ComboBoxes { get; private set; }
        public List<ToolStripButton> Buttons { get; private set; }
        public TrackBar TrackBar { get; private set; }

        public LayoutManager(DockPanel panel, ToolStripComboBox[] comboBoxes, List<ToolStripButton> buttons, TrackBar trackBar)
        {
            Panel = panel;
            ComboBoxes = comboBoxes;
            Buttons = buttons;
            TrackBar = trackBar;

            TreeView = new TreeViewFrame();
            TreeView.SelectedDatabaseChanged += TreeView_SlectedDatabaseChanged;
            TreeView.PropertiesClick += TreeView_PropertiesClick;
            StepFrames = new Dictionary<TestMethod, StepFrame>();
            LogFrame = new LogFrame();
            InstanceProperites = new BenchmarkInstanceProperies();
            InstanceProperites.Caller = TreeView;

            Initialize();

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
                StepFrames[method] = CreateStepFrame(method);
        }

        private void TreeView_PropertiesClick(object sender, EventArgs e)
        {
            ShowProperties();
        }

        public void Initialize()
        {
            TrackBar.Value = 20;
            ComboBoxes[0].SelectedIndex = 0;
            ComboBoxes[1].SelectedIndex = 5;
        }

        public void InitializeCharts()
        {
            StepFrame stepFrame;

            // Clear and prepare charts.
            foreach (var item in StepFrames)
            {
                stepFrame = item.Value;

                stepFrame.ClearCharts();
                stepFrame.InitializeCharts(TreeView.GetSelectedBenchmarks().Select(x => new KeyValuePair<string, Color>(x.Name, x.Color)));
            }
        }

        public void ResetDocking()
        {
            TreeView.DockState = DockState.DockLeft;
            InstanceProperites.DockState = DockState.DockRightAutoHide;

            foreach (var item in StepFrames)
                item.Value.DockState = DockState.Document;

            LogFrame.DockState = DockState.DockBottomAutoHide;
        }

        public void Reset()
        {
            TreeView.Dispose();

            CreateTreeView();
            ResetStepFrames();

            StepFrames[TestMethod.Write].Activate();

            LogFrame.Dispose();
            LogFrame = new LogFrame();

            LogFrame.Show(Panel);
            LogFrame.DockState = DockState.DockBottomAutoHide;
            LogFrame.Text = "Logs";

            InstanceProperites.Dispose();
            InstanceProperites = new BenchmarkInstanceProperies();

            InstanceProperites.Show(Panel);
            InstanceProperites.DockState = DockState.DockRightAutoHide;
            InstanceProperites.Text = "Properties";
            InstanceProperites.Caller = TreeView;

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
            InstanceProperites.Text = "Properties";
        }

        public bool IsCategoryOrder()
        {
          return  TreeView.IsCategoryOrder();
        }

        public void SelectFrame(TestMethod method)
        {
            StepFrame frame = StepFrames[method];
            frame.Show(Panel);
        }

        public void SelectTreeView(bool visible)
        {
            if (!visible)
            {
                if (!TreeView.IsDisposed)
                    TreeView.Hide();

                return;
            }

            if (TreeView.IsDisposed)
            {
                CreateTreeView();

                return;
            }

            TreeView.Activate();
            TreeView.Show(Panel);
        }

        public Dictionary<string, bool> GetToolStripCheckedButtons()
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            foreach (var btn in Buttons)
                result.Add(btn.Name, btn.Checked);

            return result;
        }

        public Dictionary<string, string> GetSelectedFromComboBoxes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var comboBox in ComboBoxes)
                result.Add(comboBox.Name, comboBox.Text);

            return result;
        }

        public StepFrame GetActiveStepFrame()
        {
            StepFrame activeFrame = StepFrames.FirstOrDefault(x => x.Value.IsActivated).Value;

            return activeFrame;
        }

        public void EnablePropertyFrame(bool state)
        {
            InstanceProperites.Enabled = state;
        }
        public void RefreshPropertyFrame()
        {
            InstanceProperites.SetProperties(TreeView.GetSelectedDatabase());
        }

        public void ShowProperties()
        {
            if (!InstanceProperites.IsDisposed)
            {
                InstanceProperites.Show(Panel);
                InstanceProperites.DockState = DockState.DockRight;
            }
            else
            {
                var database = TreeView.GetSelectedDatabase();
                if (database != null)
                {
                    InstanceProperites = new BenchmarkInstanceProperies();
                    InstanceProperites.Text = "Properties";

                    InstanceProperites.Show(Panel);
                    InstanceProperites.DockState = DockState.DockRight;

                    InstanceProperites.Caller = TreeView;
                    InstanceProperites.SetProperties(database);
                }
            }
        }

        public void ShowBarChart(int column, bool visible)
        {
            foreach (var kv in StepFrames)
                kv.Value.ShowBarChart(column, visible);
        }

        public void ShowLogFrame()
        {
            if (!LogFrame.IsDisposed)
                LogFrame.Show(Panel);
            else
            {
                LogFrame = new Frames.LogFrame();
                LogFrame.Show(Panel);
                LogFrame.DockState = DockState.DockBottomAutoHide;
                LogFrame.Text = "Logs";
            }
        }

        public void ClearLog()
        {
            LogFrame.Clear();
        }

        public void ClearCharts()
        {
            foreach (var frame in StepFrames)
                frame.Value.ClearCharts();
        }

        public bool IsSelectedTreeViewNode
        {
            get { return TreeView.IsSelectedBenchamrkNode; }
        }

        # region Private members

        private void CreateTreeView()
        {
            TreeView = new TreeViewFrame();
            TreeView.CreateTreeView();
            TreeView.Text = "Databases";

            TreeView.SelectedDatabaseChanged += TreeView_SlectedDatabaseChanged;
            TreeView.PropertiesClick +=TreeView_PropertiesClick;
            TreeView.Show(Panel);
            TreeView.DockState = DockState.DockLeft;
            TreeView.ExpandAll();
        }

        private void TreeView_SlectedDatabaseChanged(Object obj)
        {
            if (!InstanceProperites.IsDisposed)
                InstanceProperites.SetProperties(obj);
        }

        private StepFrame CreateStepFrame(TestMethod method)
        {
            StepFrame stepFrame = new StepFrame();
            stepFrame.Text = method.ToString();
            stepFrame.Dock = DockStyle.Fill;

            switch (method)
            {
                case TestMethod.Write:
                    stepFrame.Icon = Properties.Resources.w_24x24;
                    break;

                case TestMethod.Read:
                    stepFrame.Icon = Properties.Resources.r_24x24;
                    break;

                case TestMethod.SecondaryRead:
                    stepFrame.Icon = Properties.Resources.sr_24x24;
                    break;

                default:
                    break;
            }

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

            if (persistString == typeof(BenchmarkInstanceProperies).ToString())
                return InstanceProperites;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if (Count == 0)
                    frame = StepFrames[TestMethod.Write];
                else if (Count == 1)
                    frame = StepFrames[TestMethod.Read];
                else if (Count == 2)
                    frame = StepFrames[TestMethod.SecondaryRead];

                Count++;
            }

            return frame;
        }

        private void ResetStepFrames()
        {
            // TODO: Finding another way to restore the docking. 
            foreach (var item in StepFrames)
                item.Value.Dispose();

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
            {
                StepFrames[method] = CreateStepFrame(method);
                SelectFrame(method);
            }
        }
        #endregion
    }
}
