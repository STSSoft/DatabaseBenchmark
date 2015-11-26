using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Core;
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
        public List<StepFrame> StepFrames { get; private set; }
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

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DockConfigPath = Path.Combine(path, Settings.Default.DockingConfigurationPath);

            //TreeView = new TreeViewFrame();

            //TreeView.SelectedDatabaseChanged += TreeView_SlectedDatabaseChanged;
            //TreeView.PropertiesClick += TreeView_PropertiesClick;

            StepFrames = new List<StepFrame>();
            LogFrame = new LogFrame();

            //PropertiesFrame = new PropertiesFrame();
            //PropertiesFrame.Caller = TreeView;

            //foreach (TestMethod method in GetTestMethods())
            //    StepFrames[method] = CreateStepFrame(method);
        }

        public void Initialize()
        {
        }

        //public void SetCurrentMethod(string method)
        //{
        //    CurrentMethod = method;
        //}


        #region Frames

        #endregion

        #region Private members

        //private TestMethod[] GetTestMethods()
        //{
        //    return Enum.GetValues(typeof(TestMethod)).Cast<TestMethod>().Where(item => item != TestMethod.None).ToArray();
        //}

        #endregion
    }
}