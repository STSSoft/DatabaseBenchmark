using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Report
{
    /// <summary>
    /// Represents a description of a user.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// The email of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Any additional info.
        /// </summary>
        public string AdditionalInfo { get; set; }

        public UserInfo(string email, string additionalInfo)
        {
            Email = email;
            AdditionalInfo = additionalInfo;
        }

        public UserInfo()
        {
        }
    }

    /// <summary>
    /// Represents a description of a computer configuration.
    /// </summary>
    public class ComputerConfiguration
    {
        public OperatingSystemInfo OperatingSystem { get; set; }
        public List<CpuInfo> Processors { get; set; }
        public List<RamInfo> MemoryModules { get; set; }
        public List<StorageDeviceInfo> StorageDevices { get; set; }

        public ComputerConfiguration()
        {
        }
    }

    /// <summary>
    /// Represents a description of the OS (Operating System).
    /// </summary>
    public class OperatingSystemInfo
    {
        /// <summary>
        /// Name of the operating system.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates if the operating system is 64bit.
        /// </summary>
        public bool Is64bit { get; set; }

        public OperatingSystemInfo()
        {
        }

        public override string ToString()
        {
            string type = Is64bit ? "64bit" : "32bit";

            return String.Format("Name:{0}, Type:{1}", Name, type);
        }
    }

    /// <summary>
    /// Represents a description of a CPU (Central Processing Unit).
    /// </summary>
    public class CpuInfo
    {
        /// <summary>
        /// The name of the Central Processing Unit.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Number of threads (physical cores + logical cores).
        /// </summary>
        public int Threads { get; set; }

        /// <summary>
        /// The maximum clock speed in MHz.
        /// </summary>
        public int MaxClockSpeed { get; set; }

        public CpuInfo()
        {
        }

        public override string ToString()
        {
            return String.Format("Name:{0}, Threads:{1}, Max Clock:{2} MHz", Name, Threads, MaxClockSpeed);
        }
    }

    /// <summary>
    /// Represents a description of a single RAM module.
    /// </summary>
    public class RamInfo
    {
        /// <summary>
        /// Type of the memory: SDRAM, DDR, DDR2 or DDR3
        /// </summary>
        public MemoryType MemoryType { get; set; }

        /// <summary>
        /// Memory capacity in Gigabytes.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Memory speed in MHz.
        /// </summary>
        public int Speed { get; set; }

        public RamInfo()
        {
        }

        public override string ToString()
        {
            return String.Format("Type:{0}, Capacity:{1} MB, Speed:{2} MHz", MemoryType, Capacity, Speed);
        }

        public static MemoryType ObtainType(uint type)
        {
            if (type == 17)
                return MemoryType.SDRAM;
            else if (type == 20)
                return MemoryType.DDR;
            else if (type == 21)
                return MemoryType.DDR2;
            else if (type == 0)
                return MemoryType.DDR3; // we assume that if the memory is not SDRAM, DDR or DDR2 it is DDR3

            return MemoryType.Unknown;
        }
    }

    /// <summary>
    /// Represents a description of a storage device.
    /// </summary>
    public class StorageDeviceInfo
    {
        /// <summary>
        /// The model of the storage device.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// The size of the storage device in Gigabytes.
        /// </summary>
        public ulong Size { get; set; }

        /// <summary>
        /// The drive letters associated with this device.
        /// </summary>
        public List<string> DriveLetters { get; set; }

        public StorageDeviceInfo()
        {
        }

        public override string ToString()
        {
            return String.Format("Model:{0}, Size:{1}", Model, Size);
        }
    }

    public enum MemoryType
    {
        Unknown,
        SDRAM,
        DDR,
        DDR2,
        DDR3
    }
}
