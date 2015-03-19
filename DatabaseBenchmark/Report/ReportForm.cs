using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Report;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace DatabaseBenchmark.Validation
{
    public partial class ReportForm : Form
    {
        private ILog Logger;
        private ComputerConfiguration Configuration;
        private ServerConnection ServerConnector;

        public List<BenchmarkTest> BenchmarkTests { get; private set; }

        public ReportForm(List<BenchmarkTest> benchmarkTests)
            : this()
        {
            Logger = LogManager.GetLogger(Properties.Settings.Default.ApplicationLogger);
            BenchmarkTests = benchmarkTests;

            PopulateHardwareInfo();
        }

        public ReportForm()
        {
            InitializeComponent();
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

            // STORAGE
            string benchmarkDataDirectoryRoot = Path.GetPathRoot(BenchmarkTests.First().Database.DataDirectory);
            StorageDeviceInfo dataDrive = storage.Find(drive => drive.DriveLetters.Contains(benchmarkDataDirectoryRoot.Trim('\\')));

            comboBoxStorageModel.Items.AddRange(storage.Select(device => device.Model).ToArray());
            int selectedIndex = comboBoxStorageModel.Items.IndexOf(dataDrive.Model);
            comboBoxStorageModel.SelectedIndex = selectedIndex;

            txtBoxHddSize.Text = String.Format("{0} GB", dataDrive.Size);
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            UserInfo user = new UserInfo(txtBoxEmail.Text, txtBoxAdditionalInfo.Text);
            
            try
            {
                Configuration.StorageDevices.RemoveAll(device => device.Model != comboBoxStorageModel.Text);

                string jsonData = JsonUtils.ConvertToJson(user, Configuration, BenchmarkTests).ToString();
                ServerConnector = new ServerConnection();

                Logger.Info("Sending data to server...");
                HttpStatusCode responseCode = ServerConnector.SendData(jsonData);

                if (responseCode == HttpStatusCode.OK)
                {
                    Logger.Info("Data sent succesfully to server...");

                    MessageBox.Show("Data sent successfully to server.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // TODO: Add the real link.
                    Process.Start("http://presence.bg/bm/public/benchmark/13/view");

                    this.Close();
                }
                else
                {
                    Logger.Info("Send error...");
                    MessageBox.Show("The server is not responding at the moment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }    
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
