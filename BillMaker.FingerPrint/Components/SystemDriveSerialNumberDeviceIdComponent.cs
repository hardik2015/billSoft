using System;
using System.Management;

namespace BillMaker.FingerPrint.Components
{
    /// <summary>
    /// An implementation of <see cref="IFingerPrintComponent"/> that uses the system drive's serial number.
    /// </summary>
    public class SystemDriveSerialNumberFingerPrintComponent : IFingerPrintComponent
    {
        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public string Name { get; } = "SystemDriveSerialNumber";

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDriveSerialNumberFingerPrintComponent"/> class.
        /// </summary>
        public SystemDriveSerialNumberFingerPrintComponent() { }

        /// <summary>
        /// Gets the component value.
        /// </summary>
        /// <returns>The component value.</returns>
        public string GetValue()
        {
            var systemLogicalDiskFingerPrint = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2);

            var queryString = $"SELECT * FROM Win32_LogicalDisk where DeviceId = '{systemLogicalDiskFingerPrint}'";
            using var searcher = new ManagementObjectSearcher(queryString);

            foreach (ManagementObject disk in searcher.Get())
            {
                foreach (ManagementObject partition in disk.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject drive in partition.GetRelated("Win32_DiskDrive"))
                    {
                        var serialNumber = drive["SerialNumber"] as string;
                        return serialNumber;
                    }
                }
            }

            return null;
        }
    }
}
