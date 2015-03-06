using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace DatabaseBenchmark.Validation
{
    public static class SystemUtils
    {
        private static ILog Logger;

        static SystemUtils()
        {
            Logger = LogManager.GetLogger("ApplicationLogger");
        }

        /// <summary>
        /// Gets a detailed information about the computer configuration.
        /// </summary>
        public static ComputerConfiguration GetComputerConfiguration()
        {
            var os = GetOperatingSystem();
            var processors = GetProcessors();
            var memoryBanks = GetMemory();
            var storages = GetStorageDevices();

            ComputerConfiguration configuration = new ComputerConfiguration();
            configuration.OperatingSystem = os;
            configuration.Processors = processors;
            configuration.MemoryModules = memoryBanks;
            configuration.StorageDevices = storages;

            return configuration;
        }

        /// <summary>
        /// Gets information about the operating system.
        /// </summary>
        public static OperatingSystemInfo GetOperatingSystem()
        {
            OperatingSystemInfo system = null;

            ManagementObjectSearcher processorQuery = new ManagementObjectSearcher(

                   @"SELECT
                        Caption              
                    FROM Win32_OperatingSystem"
            );

            try
            {
                ManagementObjectCollection processorInfo = processorQuery.Get();

                system = new OperatingSystemInfo();
                system.Is64bit = Environment.Is64BitOperatingSystem;

                foreach (ManagementObject item in processorInfo)
                    system.Name = item["Caption"].ToString();
            }
            catch (Exception exc)
            {
                Logger.Error("GetCpuInfo() error...", exc);
                system = null;
            }

            return system;
        }

        /// <summary>
        /// Gets a list of the current processors installed on the machine.
        /// </summary>
        public static List<CpuInfo> GetProcessors()
        {
            List<CpuInfo> processors = null;

            ManagementObjectSearcher processorQuery = new ManagementObjectSearcher(

                   @"SELECT
                        Name,
                        NumberOfLogicalProcessors,
                        MaxClockSpeed                 
                    FROM Win32_Processor"
            );

            try
            {
                ManagementObjectCollection processorInfo = processorQuery.Get();

                processors = new List<CpuInfo>();
                foreach (ManagementObject item in processorInfo)
                {
                    CpuInfo cpu = new CpuInfo();

                    cpu.Name = item["Name"].ToString();
                    cpu.Threads = Int32.Parse(item["NumberOfLogicalProcessors"].ToString());
                    cpu.MaxClockSpeed = Int32.Parse(item["MaxClockSpeed"].ToString());

                    processors.Add(cpu);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("GetCpuInfo() error...", exc);
                processors = null;
            }

            return processors;
        }

        /// <summary>
        /// Gets a list of the current installed memory banks on the machine.
        /// </summary>
        public static List<RamInfo> GetMemory()
        {
            List<RamInfo> memoryModules = null;

            ManagementObjectSearcher memoryQuery = new ManagementObjectSearcher(

                    @"SELECT
                        MemoryType,
                        Capacity,
                        Speed
                    FROM Win32_PhysicalMemory "
            );

            try
            {
                ManagementObjectCollection memoryInfo = memoryQuery.Get();

                memoryModules = new List<RamInfo>();
                foreach (var item in memoryInfo)
                {
                    RamInfo ram = new RamInfo();

                    ram.MemoryType = RamInfo.ObtainType((UInt32.Parse(item["MemoryType"].ToString())));

                    ram.Capacity = (int)(UInt64.Parse(item["Capacity"].ToString()) / 1073741824);
                    ram.Speed = Int32.Parse(item["Speed"].ToString());

                    memoryModules.Add(ram);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("GetMemoryInfo() error...", exc);
                memoryModules = null;
            }

            return memoryModules;
        }

        /// <summary>
        /// Gets a list of the storage devices installed on the machine.
        /// </summary>
        public static List<StorageDeviceInfo> GetStorageDevices()
        {
            List<StorageDeviceInfo> storages = null;

            try
            {
                ManagementClass wmiClass = new ManagementClass(@"Win32_DiskDrive");
                ManagementObjectCollection storageDrives = wmiClass.GetInstances();

                storages = new List<StorageDeviceInfo>();
                foreach (ManagementObject disk in storageDrives)
                {
                    if (Int32.Parse(disk["Partitions"].ToString()) == 0) // Not a hard drive.
                        continue;

                    StorageDeviceInfo storage = new StorageDeviceInfo();

                    storage.Model = disk["Model"].ToString();
                    storage.Size = (UInt64.Parse(disk["Size"].ToString()) / 1073741824);
                    storage.DriveLetters = new List<string>();

                    foreach (ManagementObject diskPartition in disk.GetRelated("Win32_DiskPartition"))
                    {
                        foreach (ManagementBaseObject logicalPartition in diskPartition.GetRelated("Win32_LogicalDisk"))
                        {
                            storage.DriveLetters.Add(logicalPartition["Name"].ToString());
                        }
                    }

                    storages.Add(storage);
                } 
            }
            catch (Exception exc)
            {
                Logger.Error("GetHddInfo() error...", exc);
                storages = null;
            }

            return storages;
        }
    }
}