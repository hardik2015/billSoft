using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BillMaker.DataLib;
using Newtonsoft.Json;
using System.Net.Http;
using log4net;
using Microsoft.Win32;
using BillMaker.FingerPrint;
using System.Globalization;

namespace BillMaker
{
	class LicenseCheck
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public List<String> licenseResult;
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
                if(!IsSecondHash)
                { 
                    return hashString; 
                }
                string expiryDate = dbEntities.CompanySettings.Where(x => x.Name == "ExpiryDate").FirstOrDefault().Value;
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

        public async Task VerifyProdutAsync()
        {
            LoadIpAddressAndProductKey();
            string requestArgument = GetRequestHash(false);
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("Email",_email);
            keyValuePairs.Add("Argument", requestArgument);
            var requestData = JsonConvert.SerializeObject(keyValuePairs);
            var data = new StringContent(requestData, Encoding.UTF8, "application/json");
            string url = "https://licancesmanger.000webhostapp.com/licenceChecker.php";
            HttpClient client = new HttpClient();
			try
			{
                var response = await client.PostAsync(url, data);
                string responseResult = response.Content.ReadAsStringAsync().Result;
                licenseResult = JsonConvert.DeserializeObject<List<String>>(responseResult);
            }
            catch(HttpRequestException exc)
			{
                log.Error("Error:: while connecting : " + exc.ToString());
			}
            catch (TaskCanceledException exc)
            {
                log.Error("Error:: coonection timeout : " + exc.ToString());
            }
            catch(Exception exc)
			{
                log.Error("Error :: Connection Error : " + exc.ToString());
			}
        }

        public bool VerifyProdutLocal()
        {
            LoadIpAddressAndProductKey();
            string mData = dbEntities.CompanySettings.Where(x => x.Name.Equals("mData")).FirstOrDefault().Value;
            if(mData.Equals(_ipAddress))
			{
                String xDate = dbEntities.CompanySettings.Where(x => x.Name.Equals("xDate")).FirstOrDefault().Value;
                xDate = Encoding.UTF8.GetString(Convert.FromBase64String(xDate));
                List<string> dateValue = xDate.Split('.').ToList();
                DateTime lastCheck,expiryDate;
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime.TryParseExact(dateValue[2], "dd/MM/yyyy", provider, DateTimeStyles.AssumeLocal, out lastCheck);
                DateTime.TryParseExact(dateValue[1], "dd/MM/yyyy", provider, DateTimeStyles.AssumeLocal, out expiryDate);
                if (lastCheck.Date.AddDays(15) > DateTime.Now.Date && expiryDate.Date > DateTime.Now)
				{
                    UpdateLocalData();
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

        public void UpdateLocalData()
		{
            String xDate = "";
            if (licenseResult != null)
            {
                dbEntities.CompanySettings.Where(x => x.Name.Equals("ExpiryDate")).FirstOrDefault().Value = licenseResult[0];
                xDate = licenseResult[0] + "." + licenseResult[2] + "." + DateTime.Now.Date.ToString("dd/MM/yyyy");
            }
            else
            {
                xDate = dbEntities.CompanySettings.Where(x => x.Name.Equals("xDate")).FirstOrDefault().Value;
                xDate = Encoding.UTF8.GetString(Convert.FromBase64String(xDate));
                List<string> dateValue = xDate.Split('.').ToList();
                xDate = dateValue[0] + "." + dateValue[1] + "." + DateTime.Now.Date.ToString("dd/MM/yyyy");
            }
            xDate = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(xDate));
            dbEntities.CompanySettings.Where(x => x.Name.Equals("xDate")).FirstOrDefault().Value = xDate;
            dbEntities.CompanySettings.Where(x => x.Name.Equals("mData")).FirstOrDefault().Value = _ipAddress;
            dbEntities.SaveChanges();
        }
    }
}
