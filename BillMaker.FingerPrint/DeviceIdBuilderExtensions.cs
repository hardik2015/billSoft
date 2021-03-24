using System;
using BillMaker.FingerPrint.CommandExecutors;
using BillMaker.FingerPrint.Components;

namespace BillMaker.FingerPrint
{
    /// <summary>
    /// Extension methods for <see cref="FingerPrintBuilder"/>.
    /// </summary>
    public static class FingerPrintBuilderExtensions
    {
        /// <summary>
        /// Use the specified formatter.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to use the formatter.</param>
        /// <param name="formatter">The <see cref="IFingerPrintFormatter"/> to use.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder UseFormatter(this FingerPrintBuilder builder, IFingerPrintFormatter formatter)
        {
            builder.Formatter = formatter;
            return builder;
        }

        /// <summary>
        /// Adds the specified component to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <param name="component">The <see cref="IFingerPrintComponent"/> to add.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddComponent(this FingerPrintBuilder builder, IFingerPrintComponent component)
        {
            builder.Components.Add(component);
            return builder;
        }

        /// <summary>
        /// Adds the current user name to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddUserName(this FingerPrintBuilder builder)
        {
            return builder.AddComponent(new FingerPrintComponent("UserName", Environment.UserName));
        }

        /// <summary>
        /// Adds the machine name to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddMachineName(this FingerPrintBuilder builder)
        {
            return builder.AddComponent(new FingerPrintComponent("MachineName", Environment.MachineName));
        }

        /// <summary>
        /// Adds the operating system version to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddOSVersion(this FingerPrintBuilder builder)
        {
            return builder.AddComponent(new FingerPrintComponent("OSVersion", OS.Version));
        }

        /// <summary>
        /// Adds the MAC address to the device identifier, optionally excluding non-physical adapters and/or wireless adapters.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <param name="excludeNonPhysical">A value indicating whether non-physical adapters should be excluded.</param>
        /// <param name="excludeWireless">A value indicating whether wireless adapters should be excluded.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddMacAddress(this FingerPrintBuilder builder, bool excludeNonPhysical = false, bool excludeWireless = false)
        {
            return builder.AddComponent(new NetworkAdapterFingerPrintComponent(excludeNonPhysical, excludeWireless));
        }

        /// <summary>
        /// Adds the processor ID to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddProcessorId(this FingerPrintBuilder builder)
        {
            if (OS.IsWindows)
            {
                return builder.AddComponent(new WmiFingerPrintComponent("ProcessorId", "Win32_Processor", "ProcessorId"));
            }
            else if (OS.IsLinux)
            {
                return builder.AddComponent(new FileFingerPrintComponent("ProcessorId", "/proc/cpuinfo", true));
            }
            else
            {
                return builder.AddComponent(new UnsupportedFingerPrintComponent("ProcessorId"));
            }
        }

        /// <summary>
        /// Adds the motherboard serial number to the device identifier. On Linux, this requires root privilege.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddMotherboardSerialNumber(this FingerPrintBuilder builder)
        {
            if (OS.IsWindows)
            {
                return builder.AddComponent(new WmiFingerPrintComponent("MotherboardSerialNumber", "Win32_BaseBoard", "SerialNumber"));
            }
            else if (OS.IsLinux)
            {
                return builder.AddComponent(new FileFingerPrintComponent("MotherboardSerialNumber", "/sys/class/dmi/id/board_serial"));
            }
            else
            {
                return builder.AddComponent(new UnsupportedFingerPrintComponent("MotherboardSerialNumber"));
            }
        }

        /// <summary>
        /// Adds the system drive's serial number to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddSystemDriveSerialNumber(this FingerPrintBuilder builder)
        {
            if (OS.IsWindows)
            {
                return builder.AddComponent(new SystemDriveSerialNumberFingerPrintComponent());
            }
            else if (OS.IsLinux)
            {
                return builder.AddComponent(new LinuxRootDriveSerialNumberFingerPrintComponent());
            }
            else if (OS.IsOSX)
            {
                return builder.AddComponent(new CommandComponent(
                    name: "SystemDriveSerialNumber",
                    command: "system_profiler SPSerialATADataType | sed -En 's/.*Serial Number: ([\\d\\w]*)//p'",
                    commandExecutor: CommandExecutor.Bash));
            }
            else
            {
                return builder.AddComponent(new UnsupportedFingerPrintComponent("SystemDriveSerialNumber"));
            }
        }

        /// <summary>
        /// Adds the system UUID to the device identifier. On Linux, this requires root privilege.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddSystemUUID(this FingerPrintBuilder builder)
        {
            if (OS.IsWindows)
            {
                return builder.AddComponent(new WmiFingerPrintComponent("SystemUUID", "Win32_ComputerSystemProduct", "UUID"));
            }
            else if (OS.IsLinux)
            {
                return builder.AddComponent(new FileFingerPrintComponent("SystemUUID", "/sys/class/dmi/id/product_uuid"));
            }
            else
            {
                return builder.AddComponent(new UnsupportedFingerPrintComponent("SystemUUID"));
            }
        }

        /// <summary>
        /// Adds the identifier tied to the installation of the OS.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddOSInstallationID(this FingerPrintBuilder builder)
        {
            if (OS.IsWindows)
            {
                return builder.AddComponent(new RegistryValueFingerPrintComponent("OSInstallationID", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid"));
            }
            else if (OS.IsLinux)
            {
                return builder.AddComponent(new FileFingerPrintComponent("OSInstallationID", new string[] { "/var/lib/dbus/machine-id", "/etc/machine-id" }));
            }
            else if (OS.IsOSX)
            {
                return builder.AddComponent(new CommandComponent(
                    name: "OSInstallationID",
                    command: "ioreg -l | grep IOPlatformSerialNumber | sed 's/.*= //' | sed 's/\"//g'",
                    commandExecutor: CommandExecutor.Bash));
            }
            else
            {
                return builder.AddComponent(new UnsupportedFingerPrintComponent("OSInstallationID"));
            }
        }

        /// <summary>
        /// Adds a registry value to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <param name="name">The name of the component.</param>
        /// <param name="key">The full path of the registry key.</param>
        /// <param name="valueName">The name of the registry value.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddRegistryValue(this FingerPrintBuilder builder, string name, string key, string valueName)
        {
            return builder.AddComponent(new RegistryValueFingerPrintComponent(name, key, valueName));
        }

        /// <summary>
        /// Adds a file-based token to the device identifier.
        /// </summary>
        /// <param name="builder">The <see cref="FingerPrintBuilder"/> to add the component to.</param>
        /// <param name="path">The path of the token.</param>
        /// <returns>The <see cref="FingerPrintBuilder"/> instance.</returns>
        public static FingerPrintBuilder AddFileToken(this FingerPrintBuilder builder, string path)
        {
            var name = string.Concat("FileToken", path.GetHashCode());
            return builder.AddComponent(new FileTokenFingerPrintComponent(name, path));
        }
    }
}
