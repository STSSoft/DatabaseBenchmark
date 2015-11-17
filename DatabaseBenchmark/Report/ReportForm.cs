using DatabaseBenchmark.Core;
using DatabaseBenchmark.Properties;
using DatabaseBenchmark.Reporting;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace DatabaseBenchmark.Report
{
    public partial class ReportForm : Form
    {
        private ILog Logger;

        private ComputerConfiguration Configuration;
        private ServerConnection ServerConnector;

        public List<Benchmark> BenchmarkSessions { get; private set; }

        public ReportForm(List<Benchmark> benchmarkSessions)
            : this()
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
            BenchmarkSessions = benchmarkSessions;

            PopulateHardwareInfo();
        }

        public ReportForm()
        {
            InitializeComponent();

            checkBoxShow.Checked = (bool)Settings.Default.HideReportForm;
        }

        private void PopulateHardwareInfo()
        {
            Configuration = SystemUtils.GetComputerConfiguration();

            OperatingSystemInfo operatingSystem = Configuration.OperatingSystem;
            List<CpuInfo> processors = Configuration.Processors;
            List<RamInfo> memory = Configuration.MemoryModules;
            List<StorageDeviceInfo> storage = Configuration.StorageDevices;

            // OS
            txtBoxOsName.Text = operatingSystem.Name;
            txtBoxOsType.Text = operatingSystem.Is64bit ? "64 bit" : "32 bit";

            // CPU
            CpuInfo cpu = processors.First();
            txtBoxCpuName.Text = cpu.Name;
            txtBoxCpuFrequency.Text = String.Format("{0} MHz", cpu.MaxClockSpeed);
            txtBoxCpuThreads.Text = cpu.Threads.ToString();
            txtBoxCpuCount.Text = processors.Count.ToString();

            // RAM
            RamInfo ram = memory.First();

            int capacity = 0;
            foreach (var bank in memory)
                capacity += bank.Capacity;

            txtBoxMemoryCapacity.Text = String.Format("{0} GB", capacity);
            txtBoxMemoryType.Text = ram.MemoryType.ToString();
            txtBoxMemoryFrequency.Text = String.Format("{0} MHz", ram.Speed);
            txtBoxMemoryBanks.Text = memory.Count.ToString();

            // TODO: Fix this.
            // STORAGE
            //string benchmarkDataDirectoryRoot = Path.GetPathRoot(BenchmarkSessions.First().Database.DataDirectory);
            //StorageDeviceInfo dataDrive = storage.Find(drive => drive.DriveLetters.Contains(benchmarkDataDirectoryRoot.Trim('\\')));

            //comboBoxStorageModel.Items.AddRange(storage.Select(device => device.Model).ToArray());
            //int selectedIndex = comboBoxStorageModel.Items.IndexOf(dataDrive.Model);
            //comboBoxStorageModel.SelectedIndex = selectedIndex;

            //txtBoxHddSize.Text = String.Format("{0} GB", dataDrive.Size);
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            UserInfo user = new UserInfo(txtBoxEmail.Text, txtBoxAdditionalInfo.Text);

            Settings.Default.HideReportForm = checkBoxShow.Checked;
            Settings.Default.Save();
            
            try
            {
                Configuration.StorageDevices.RemoveAll(device => device.Model != comboBoxStorageModel.Text);

                // TODO: fix this method to return string.
                //string jsonData = JsonUtils.ConvertToJson(user, Configuration, BenchmarkSessions).ToString();
                //ServerConnector = new ServerConnection();

                //Logger.Info("Sending data to server...");

                //HttpWebResponse response = ServerConnector.SendDataAsPost(jsonData);
                //StreamReader reader = new StreamReader(response.GetResponseStream());

                //string benchmarkLink = reader.ReadLine();

                //if (response.StatusCode == HttpStatusCode.OK)
                //{
                //    Logger.Info("Data sent succesfully to server...");
                //    MessageBox.Show("Data sent successfully to server.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //    Process.Start(benchmarkLink);

                //    this.Close();
                //}
                //else
                //{
                //    Logger.Info(String.Format("Send error..."));
                //    Logger.Info(String.Format("Server return code: {0}", response.StatusCode));

                //    MessageBox.Show(String.Format("There was an error. The server returned code: {0}", response.StatusCode.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}    
            }
            catch (Exception exc)
            {
                Logger.Error("Send error...", exc);
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();

            Settings.Default.HideReportForm = checkBoxShow.Checked;
            Settings.Default.Save();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cbUserDefined.Checked)
            {
                foreach (var control in groupBoxOS.Controls.OfType<TextBox>())
                    control.ReadOnly = false;

                foreach (var control in groupBoxStorage.Controls.OfType<TextBox>())
                    control.ReadOnly = false;

                foreach (var control in groupBoxConfiguration.Controls.OfType<TextBox>())
                    control.ReadOnly = false;
            }
            else
            {
                foreach (var control in groupBoxOS.Controls.OfType<TextBox>())
                    control.ReadOnly = true;

                foreach (var control in groupBoxStorage.Controls.OfType<TextBox>())
                    control.ReadOnly = true;

                foreach (var control in groupBoxConfiguration.Controls.OfType<TextBox>())
                    control.ReadOnly = true;
            }
        }

        private void comboBoxStorageModel_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string newSize = Configuration.StorageDevices.Find(device => device.Model.Equals((string)comboBoxStorageModel.SelectedItem)).Size.ToString();
            txtBoxHddSize.Text = String.Format("{0} GB", newSize);
        }
    }
}
