using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using BillMaker.DataConnection;
using Newtonsoft.Json;
using System.Net.Http;
using log4net;
using DeviceId;
using DeviceId.Formatters;
using DeviceId.Encoders;

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
        MyAttachedDbEntities dbEntities = new MyAttachedDbEntities();
        private string _productKey;
        private string _ipAddress;
        private string _email;
        public void LoadIpAddressAndProductKey() 
        {
            _ipAddress = new DeviceIdBuilder().UseFormatter(new StringDeviceIdFormatter(new PlainTextDeviceIdComponentEncoder())).AddMotherboardSerialNumber().ToString();
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
            var response = await client.PostAsync(url, data);
            string responseResult = response.Content.ReadAsStringAsync().Result;
            licenseResult = JsonConvert.DeserializeObject<List<String>>(responseResult);
            log.Info(licenseResult);

        }
    }
}
