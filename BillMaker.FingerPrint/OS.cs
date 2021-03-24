﻿using System;
using System.Runtime.InteropServices;
#if NETSTANDARD
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;
#endif

namespace BillMaker.FingerPrint
{
    /// <summary>
    /// Provides helper methods relating to the OS.
    /// </summary>
    internal static class OS
    {
        /// <summary>
        /// Gets a value indicating whether this is a Windows OS.
        /// </summary>
        public static bool IsWindows { get; }
            = true;

        /// <summary>
        /// Gets a value indicating whether this is a Linux OS.
        /// </summary>
        public static bool IsLinux { get; }
#if NETFRAMEWORK
            = false;
#elif NETSTANDARD
            = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif

        /// <summary>
        /// Gets a value indicating whether this is OS X.
        /// </summary>
        public static bool IsOSX { get; }
#if NETFRAMEWORK
            = false;
#elif NETSTANDARD
            = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif

        /// <summary>
        /// Gets the current OS version.
        /// </summary>
        public static string Version { get; }
#if NETFRAMEWORK
            = Environment.OSVersion.ToString();
#elif NETSTANDARD
            = IsWindows
                ? Environment.OSVersion.ToString()
                : string.Concat(RuntimeEnvironment.OperatingSystem, " ", RuntimeEnvironment.OperatingSystemVersion);
#endif
    }
}
