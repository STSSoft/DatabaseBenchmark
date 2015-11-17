using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Core.Benchmarking;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Serialization
{
    public class MainLayout
    {
        private int Count;
        private ILog Logger;

        private string DockConfigPath;

        public DockPanel Panel { get; private set; }
        public TreeViewFrame TreeView { get; private set; }
        public LogFrame LogFrame { get; private set; }
        //public Dictionary<TestMethod, StepFrame> StepFrames { get; private set; }
        public PropertiesFrame PropertiesFrame { get; private set; }

        public TrackBar TrackBar { get; private set; }
        public List<ToolStripComboBox> ComboBoxes { get; private set; }
        public List<ToolStripButton> Buttons { get; private set; }

        public MainLayout(DockPanel panel, List<ToolStripComboBox> comboBoxes, List<ToolStripButton> buttons, TrackBar trackBar, string path)
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);

            Panel = panel;
            ComboBoxes = comboBoxes;
            Buttons = buttons;
            TrackBar = trackBar;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DockConfigPath = Path.Combine(path, Settings.Default.DockingConfigurationPath);

            TreeView = new TreeViewFrame();

            TreeView.SelectedDatabaseChanged += TreeView_SlectedDatabaseChanged;
            TreeView.PropertiesClick += TreeView_PropertiesClick;

            StepFrames = new Dictionary<TestMethod, StepFrame>();
            LogFrame = new LogFrame();

            PropertiesFrame = new PropertiesFrame();
            PropertiesFrame.Caller = TreeView;
            
            foreach (TestMethod method in GetTestMethods())
                StepFrames[method] = CreateStepFrame(method);
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
            ShowTreeViewFrame();
            TreeView.ExpandAll();

            ShowStepFrames();
            StepFrames[TestMethod.Write].Activate();

            LogFrame.Dispose();
            ShowLogFrame();

            PropertiesFrame.Dispose();
            ShowPropertiesFrame();

            Initialize();
        }

        public void SetCurrentMethod(TestMethod method)
        {
            CurrentMethod = method;
        }

        public Dictionary<string, bool> GetCheckedToolStripButtons()
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            foreach (var btn in Buttons)
                result.Add(btn.Name, btn.Checked);

            return result;
        }

        public Dictionary<string, string> GetSelectedValuesFromComboBoxes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var comboBox in ComboBoxes)
                result.Add(comboBox.Name, comboBox.Text);

            return result;
        }

        public void SelectTreeView(bool visible)
        {
            if (!visible)
            {
                TreeView.Hide();
                return;
            }

            ShowTreeViewFrame();
        }

        public bool IsSelectedTreeViewNode
        {
            get { return TreeView.IsSelectedNodeDatabase; }
        }

        #region Docking

        public void StoreDocking()
        {
            Panel.SaveAsXml(DockConfigPath);
        }

        public void LoadDocking()
        {
            try
            {
                if (!File.Exists(DockConfigPath))
                {
                    Reset();
                    return;
                }
                Panel.LoadFromXml(DockConfigPath, new DeserializeDockContent(GetContentFromPersistString));

                TreeView.Text = "Databases";
                TreeView.ExpandAll();
                LogFrame.Text = "Logs";
                PropertiesFrame.Text = "Properties";
            }
            catch (Exception exc)
            {
                Logger.Error("Load docking configuration error...", exc);
                Reset();
            }
        }

        public void RefreshDocking()
        {
            TreeView.DockState = DockState.DockLeft;

            foreach (var item in StepFrames)
                item.Value.DockState = DockState.Document;

            if (!LogFrame.IsDisposed)
                LogFrame.DockState = DockState.DockBottomAutoHide;
            else
                ShowLogFrame();

            ShowPropertiesFrame();
        }

        #endregion

        #region Charts

        public void InitializeCharts(List<KeyValuePair<string, Color>> charts)
        {
            StepFrame stepFrame;

            // Clear and prepare charts.
            foreach (var item in StepFrames)
            {
                stepFrame = item.Value;
                stepFrame.ClearCharts();
                stepFrame.InitializeCharts(charts);
            }
        }

        public List<KeyValuePair<string, Color>> GetSelectedDatabasesChartValues()
        {
            return new List<KeyValuePair<string, Color>>(TreeView.GetSelectedDatabases().Select(x => new KeyValuePair<string, Color>(x.Name, x.Color)));
        }

        public void ShowBarChart(int column, bool visible)
        {
            foreach (var kv in StepFrames)
                kv.Value.ShowBarChart(column, visible);
        }

        public void ClearCharts()
        {
            foreach (var frame in StepFrames)
                frame.Value.ClearCharts();
        }

        #endregion

        #region Frames

        public void ShowTreeViewFrame()
        {
            if (TreeView.IsDisposed)
            {
                TreeView = new TreeViewFrame();
                TreeView.CreateTreeView();
            }

            TreeView.Activate();

            TreeView.Text = "Databases";
            TreeView.Show(Panel);

            TreeView.DockState = DockState.DockLeft;

            TreeView.SelectedDatabaseChanged += TreeView_SlectedDatabaseChanged;
            TreeView.PropertiesClick += TreeView_PropertiesClick;
        }

        public void ShowPropertiesFrame()
        {
            if (!PropertiesFrame.IsDisposed)
            {
                PropertiesFrame.Show(Panel);
                PropertiesFrame.DockState = DockState.DockRight;
            }
            else
            {
                PropertiesFrame = new PropertiesFrame();

                PropertiesFrame.Show(Panel);
                PropertiesFrame.DockState = DockState.DockRight;
                PropertiesFrame.Text = "Properties";

                var database = TreeView.GetSelectedDatabase();
                if (database != null)
                {
                    PropertiesFrame.Caller = TreeView;
                    PropertiesFrame.SetProperties(database);
                }
            }
        }

        public void ShowLogFrame()
        {
            if (!LogFrame.IsDisposed)
                LogFrame.Show(Panel);
            else
            {
                LogFrame = new LogFrame();
                LogFrame.Show(Panel);
                LogFrame.DockState = DockState.DockBottomAutoHide;
                LogFrame.Text = "Logs";
            }
        }

        private void ShowStepFrames()
        {
            // TODO: Finding another way to restore the docking. 
            foreach (var item in StepFrames)
                item.Value.Dispose();

            foreach (var method in GetTestMethods())
            {
                StepFrames[method] = CreateStepFrame(method);
                SelectFrame(method);
            }
        }

        public void SelectFrame(TestMethod method)
        {
            StepFrame frame = StepFrames[method];
            frame.Show(Panel);
        }

        public StepFrame GetCurrentFrame()
        {
            return StepFrames[CurrentMethod];
        }

        public StepFrame GetActiveFrame()
        {
            StepFrame activeFrame = StepFrames.FirstOrDefault(x => x.Value.IsActivated).Value;

            return activeFrame;
        }

        public void EnablePropertiesFrame(bool state)
        {
            PropertiesFrame.Enabled = state;
        }

        public void RefreshPropertiesFrame()
        {
            PropertiesFrame.SetProperties(TreeView.GetSelectedDatabase());
        }

        public void ClearLogFrame()
        {
            LogFrame.Clear();
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

        #endregion

        #region Private members

        private TestMethod[] GetTestMethods()
        {
            return Enum.GetValues(typeof(TestMethod)).Cast<TestMethod>().Where(item => item != TestMethod.None).ToArray();
        }

        private void TreeView_PropertiesClick(object sender, EventArgs e)
        {
            ShowPropertiesFrame();
        }

        private void TreeView_SlectedDatabaseChanged(Object obj)
        {
            if (!PropertiesFrame.IsDisposed)
                PropertiesFrame.SetProperties(obj);
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return TreeView;

            if (persistString == typeof(LogFrame).ToString())
                return LogFrame;

            if (persistString == typeof(PropertiesFrame).ToString())
                return PropertiesFrame;

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

        #endregion
    }
}
