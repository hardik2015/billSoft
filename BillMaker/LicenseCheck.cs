using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BillMaker.DataLib;
using Newtonsoft.Json;
using log4net;
using Microsoft.Win32;
using BillMaker.FingerPrint;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using BillMaker.LicenseArgs;

namespace BillMaker
{
	class LicenseCheck
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public LicenseCheck()
		{
			log4net.Config.XmlConfigurator.Configure();
		}
		BillMakerEntities dbEntities = new BillMakerEntities();
		private string _productKey;
		private string _ipAddress;
		private string _email;
		public void LoadIpAddressAndProductKey()
		{
			string MachineGuid = "";
			RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
			using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
			{
				RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", false);
				if (instanceKey != null)
				{
					MachineGuid = instanceKey.GetValue("MachineGuid") as String;
					instanceKey.Close();
				}

			}
			_ipAddress = new FingerPrintBuilder().AddSystemUUID().AddProcessorId().AddSystemDriveSerialNumber().AddMotherboardSerialNumber().AddOSInstallationID().ToString();
			_productKey = dbEntities.CompanySettings.Where(x => x.Name == "ProductKey").FirstOrDefault().Value;
			_email = dbEntities.CompanySettings.Where(x => x.Name == "RegisteredEmail").FirstOrDefault().Value;
		}

		public string GetRequestHash(bool IsSecondHash)
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				string dataString = _productKey + _ipAddress;
				byte[] hashedBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
				string hashString = "";
				for (int i = 0; i < hashedBytes.Length; i++)
				{
					hashString += hashedBytes[i].ToString("x2");
				}
				if (!IsSecondHash)
				{
					return hashString;
				}
				dataString = hashString + bool.TrueString;
				hashedBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
				hashString = "";
				for (int i = 0; i < hashedBytes.Length; i++)
				{
					hashString += hashedBytes[i].ToString("x2");
				}
				return hashString;
			}
		}

		public async Task<HttpResponseMessage> VerifyProdutAsync()
		{

			LoadIpAddressAndProductKey();
			string requestArgument = GetRequestHash(false);
			LicenseCheckingRequest checkingRequest = new LicenseCheckingRequest(_email, requestArgument);
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri("http://friendcircles.xyz/");
			try
			{
				HttpResponseMessage response = await client.PostAsJsonAsync("api/Values", checkingRequest);
				return response;
			}
			catch (Exception exc)
			{
				log.Error("Error :: Connection Error : " + exc.ToString());
				return new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
			}
		}

		public bool VerifyProdutLocal()
		{
			LoadIpAddressAndProductKey();
			string mData = dbEntities.CompanySettings.Where(x => x.Name.Equals("mData")).FirstOrDefault().Value;
			if (mData.Equals(_ipAddress))
			{
				String xDate = dbEntities.CompanySettings.Where(x => x.Name.Equals("xDate")).FirstOrDefault().Value;
				xDate = Encoding.UTF8.GetString(Convert.FromBase64String(xDate));
				List<string> dateValue = xDate.Split('.').ToList();
				DateTime lastCheck, expiryDate;
				CultureInfo provider = CultureInfo.InvariantCulture;
				DateTime.TryParseExact(dateValue[2], "dd-MM-yyyy", provider, DateTimeStyles.AssumeLocal, out lastCheck);
				DateTime.TryParseExact(dateValue[1], "dd-MM-yyyy", provider, DateTimeStyles.AssumeLocal, out expiryDate);
				if (lastCheck.Date.AddDays(15) > DateTime.Now.Date && expiryDate.Date >= DateTime.Now.Date)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				dbEntities.CompanySettings.Where(x => x.Name.Equals("mData")).FirstOrDefault().Value = _ipAddress;
				dbEntities.SaveChanges();
				return false;
			}
		}

		public void UpdateLocalData(string ExpiryDate)
		{
			String xDate = dbEntities.CompanySettings.Where(x => x.Name.Equals("xDate")).FirstOrDefault().Value;
			if (!ExpiryDate.Equals(""))
			{
				dbEntities.CompanySettings.Where(x => x.Name.Equals("ExpiryDate")).FirstOrDefault().Value = ExpiryDate;
			}
			else
			{
				xDate = Encoding.UTF8.GetString(Convert.FromBase64String(xDate));
				List<string> oldDateValue = xDate.Split('.').ToList();
				ExpiryDate = oldDateValue[1];
			}
			List<string> newDateValue = xDate.Split('.').ToList();
			xDate = newDateValue[0] + "." + ExpiryDate + "." + DateTime.Now.Date.ToString("dd-MM-yyyy");
			xDate = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(xDate));
			dbEntities.CompanySettings.Where(x => x.Name.Equals("xDate")).FirstOrDefault().Value = xDate;
			dbEntities.CompanySettings.Where(x => x.Name.Equals("mData")).FirstOrDefault().Value = _ipAddress;
			dbEntities.SaveChanges();
		}

	}
}
