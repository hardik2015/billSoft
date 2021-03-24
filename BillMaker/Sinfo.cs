﻿using System;
using System.Management;
using System.Text;
using System.ComponentModel;


namespace BillMaker
{
	/// <summary>
	/// The Fingerprint class responsible for generating locking
	/// mechanism data on the Windows platform.
	///
	/// This particular Fingerprint uses WMI for .NET in order
	/// to obtain the requested hardware data. The only primary
	/// similarity to its Linux counterpart is that it may have
	/// to derive values from peripheral data in order to obtain
	/// what is actually requested in some cases.
	///
	/// A majority of the code here is borrowed from Derek's
	/// source.
	/// </summary>
	internal class Fingerprint
	{
		public Fingerprint()
		{
		}

		public String MACAddress
		{
			get
			{
				return Fingerprint.WMIInfo("Win32_NetworkAdapterConfiguration", "MACAddress");
			}
		}

		public String CPUID
		{
			get
			{
				String val = Fingerprint.WMIInfo("Win32_Processor", "UniqueId");
				if (val == String.Empty)
				{
					val = Fingerprint.WMIInfo("Win32_Processor", "ProcessodId");
				}

				if (val == String.Empty)
				{
					val = Fingerprint.WMIInfo("Win32_Processor", "Name");
				}

				if (val == String.Empty)
				{
					val = Fingerprint.WMIInfo("Win32_Processor", "Manufacturer");
				}

				return val;
			}
		}

		public String MotherboardID
		{
			get
			{
				StringBuilder sbuilder = new StringBuilder();

				/*
				 * Pack several values into val with whitespace appended to the end of
				 * each query value so that methods like split can pull the values
				 * out into an array.
				 */
				sbuilder.Append(Fingerprint.WMIInfo("Win32_BaseBoard", "Manufacturer") + " ");
				sbuilder.Append(Fingerprint.WMIInfo("Win32_BaseBoard", "Model") + " ");
				sbuilder.Append(Fingerprint.WMIInfo("Win32_BaseBoard", "PartNumber") + " ");
				sbuilder.Append(Fingerprint.WMIInfo("Win32_BaseBoard", "SerialNumber")); // The last string doesn't need the whitespac

				return sbuilder.ToString();
			}
		}

		public String PrimaryHDDID
		{
			get
			{
				// Attempt to determine the primary HDD and get the serial number from it
				ManagementClass wmiMgmt = new ManagementClass("Win32_DiskDrive");
				ManagementObjectCollection wmiMgmtCol = wmiMgmt.GetInstances();
				String val = String.Empty;

				foreach (ManagementObject wmiMgmtObj in wmiMgmtCol)
				{
					// Get the physical device ID associated with this current device
					String deviceId = Fingerprint.WMIInfo("Win32_DiskDrive", "DeviceId");
					if (deviceId.Contains("PHYSICALDRIVE0"))
					{
						// Assume that this is the primary physical drive in the system
						val = Fingerprint.WMIInfo("Win32_DiskDrive", "SerialNumber");

						// No need to continue looping
						break;
					}
				}

				return val;
			}
		}

		public String BIOSID
		{
			get
			{
				return Fingerprint.WMIInfo("Win32_Bios", "SerialNumber");
			}
		}

		public String SystemUUID
		{
			get
			{
				return Fingerprint.WMIInfo("Win32_ComputerSystemProduct", "UUID");
			}
		}

		public String VideoCardID
		{
			get
			{
				return Fingerprint.WMIInfo("Win32_VideoController", "DeviceID");
			}
		}

		/// <summary>
		/// Obtain hardware specific information on the Windows platform.
		/// </summary>
		/// <returns>
		/// A String referring to the requested WMI Class's property
		/// </returns>
		/// <param name='win32Class'>
		/// The Win32 class to access
		/// </param>
		/// <param name='win32Property'>
		/// The property to access in the specified Win32 class
		/// </param>
		private static String WMIInfo(String win32Class, String win32Property)
		{
			ManagementClass wmiMgmt = new ManagementClass(win32Class);
			ManagementObjectCollection wmiMgmtCol = wmiMgmt.GetInstances();
			String propertyValue = String.Empty;

			// Loop through the collection to get the property
			foreach (ManagementObject wmiObject in wmiMgmtCol)
			{
				try
				{
					propertyValue = wmiObject[win32Property].ToString();
					break;
				}
				catch (Exception) { }
			}

			return propertyValue;
		}
	}
}