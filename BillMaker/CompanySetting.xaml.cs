using BillMaker.DataConnection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Drawing.Printing;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for CompanySetting.xaml
	/// </summary>
	public partial class CompanySettingPage :INotifyPropertyChanged
	{
		private MyAttachedDbEntities db = new MyAttachedDbEntities();
		CompanySetting companyName;
		CompanySetting companyPhone;
		CompanySetting companyGSTINNo;
		CompanySetting companyEmailId;
		CompanySetting companyTanNo;
		CompanySetting companyAccountNumber;
		CompanySetting companyIFSCCode;
		CompanySetting settingIsShowBankDetails;
		CompanySetting settingDefaultPrinter;
		CompanySetting companyAddress;
		public event PropertyChangedEventHandler PropertyChanged;
		string mobileNumberValidation = @"^([987]{1})(\d{1})(\d{8})";

		public CompanySettingPage()
		{
			InitializeComponent();
			this.DataContext = this;
			companyName = db.CompanySettings.Where(x => x.Name == "CompanyName").FirstOrDefault();
			companyPhone = db.CompanySettings.Where(x => x.Name == "CompanyPhone").FirstOrDefault();
			companyGSTINNo = db.CompanySettings.Where(x => x.Name == "CompanyGSTINNo").FirstOrDefault();
			companyEmailId = db.CompanySettings.Where(x => x.Name == "CompanyEmailId").FirstOrDefault();
			companyTanNo = db.CompanySettings.Where(x => x.Name == "CompanyTANNo").FirstOrDefault();
			companyAccountNumber = db.CompanySettings.Where(x => x.Name == "CompanyAccountNumber").FirstOrDefault();
			companyIFSCCode = db.CompanySettings.Where(x => x.Name == "ComapnyIFSCCode").FirstOrDefault();
			settingIsShowBankDetails = db.CompanySettings.Where(x => x.Name == "IsShowBankDetails").FirstOrDefault();
			settingDefaultPrinter = db.CompanySettings.Where(x => x.Name == "DefaultPrinter").FirstOrDefault();
			companyAddress = db.CompanySettings.Where(x => x.Name == "CompanyAddress").FirstOrDefault();
			foreach (string printname in PrinterSettings.InstalledPrinters)
			{
				PrinterNames.Items.Add(printname);
				if(settingDefaultPrinter.Value == printname)
                {
					PrinterNames.SelectedItem = printname;
                }
			}

		}



		public String CompanyName
		{
			get
			{
				return companyName.Value;
			}
			set
			{
				companyName.Value = value;
			}
		}

		public string CompanyPhone
		{
			get
			{
				return companyPhone.Value;
			}
			set
			{
				if (Regex.IsMatch(value, mobileNumberValidation))
				{
					companyPhone.Value = value;
				}
				else
					companyPhone.Value = "";
			}
		}

		public String CompanyGSTINNo
		{
			get
			{
				return companyGSTINNo.Value;
			}
			set
			{
				companyGSTINNo.Value = value;
			}
		}

		public String CompanyEmailId
		{
			get
			{
				return companyEmailId.Value;
			}
			set
			{
				companyEmailId.Value = value;
			}
		}

		public String CompanyTANNo
		{
			get
			{
				return companyTanNo.Value;
			}
			set
			{
				companyTanNo.Value = value;
			}
		}

		public string CompanyAddress
		{
			get
			{
				return companyAddress.Value;
			}
			set
			{
				companyAddress.Value = value;
			}
		}

		public String CompanyAccountNumber
		{
			get
			{
				return companyAccountNumber.Value;
			}
			set
			{
				companyAccountNumber.Value = value;
			}
		}

		public String CompanyIFSCCode
		{
			get
			{
				return companyIFSCCode.Value;
			}
			set
			{
				companyIFSCCode.Value = value;
			}
		}

		public bool SettingIsShowBankDetails
		{
			get
			{
				return settingIsShowBankDetails.Value == "1";
			}
			set
			{
				if (value)
					settingIsShowBankDetails.Value = "1";
				else
					settingIsShowBankDetails.Value = "0";
			}
		}
		
		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private async void SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			string Title = "Information";
			string MessageText = "Are you sure you wan to save settings ?";
			MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
			messageBoxDialog.IsSecondaryButtonEnabled = true;
			ContentDialogResult answer = await messageBoxDialog.ShowAsync();
			if(answer == ContentDialogResult.Primary)
				db.SaveChanges();
			
		}

        private void PrinterNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			settingDefaultPrinter.Value = (sender as ComboBox).SelectedItem as String;
		}
    }
}
