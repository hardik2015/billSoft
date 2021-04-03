using BillMaker.DataLib;
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
		private BillMakerEntities db = new BillMakerEntities();
		CompanySetting companyName;
		CompanySetting companyPhone;
		CompanySetting companyGSTINNo;
		CompanySetting companyEmailId;
		CompanySetting companyTanNo;
		CompanySetting settingIsShowBankDetails;
		CompanySetting settingDefaultPrinter;
		CompanySetting companyAddress;
		CompanySetting settingVoucherEnabled;
		public event PropertyChangedEventHandler PropertyChanged;
		string mobileNumberValidation = @"^([987]{1})(\d{1})(\d{8})";

		public CompanySettingPage()
		{
			InitializeComponent();
			BankAccount bankAccount = db.BankAccounts.Where(bank => bank.Id == 1).FirstOrDefault();
			if(bankAccount != null)
			{
				AccountBalance.Text = "Balance : " + bankAccount.Balance;
				BankName.IsEnabled = false;
				AccountNumber.IsEnabled = false;
				IFSCCode.IsEnabled = false;
				BankName.Text = bankAccount.Name;
				AccountNumber.Text = bankAccount.AcoountNo;
				IFSCCode.Text = bankAccount.IFSCCode;
			}
			this.DataContext = this;
			companyName = db.CompanySettings.Where(x => x.Name == "CompanyName").FirstOrDefault();
			companyPhone = db.CompanySettings.Where(x => x.Name == "CompanyPhone").FirstOrDefault();
			companyGSTINNo = db.CompanySettings.Where(x => x.Name == "CompanyGSTINNo").FirstOrDefault();
			companyEmailId = db.CompanySettings.Where(x => x.Name == "CompanyEmailId").FirstOrDefault();
			companyTanNo = db.CompanySettings.Where(x => x.Name == "CompanyTANNo").FirstOrDefault();
			settingIsShowBankDetails = db.CompanySettings.Where(x => x.Name == "IsShowBankDetails").FirstOrDefault();
			settingDefaultPrinter = db.CompanySettings.Where(x => x.Name == "DefaultPrinter").FirstOrDefault();
			companyAddress = db.CompanySettings.Where(x => x.Name == "CompanyAddress").FirstOrDefault();
			settingVoucherEnabled = db.CompanySettings.Where(x => x.Name == "VoucherEnabled").FirstOrDefault();
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

		public bool SettingVoucherEnabled
		{
			get
			{
				return settingVoucherEnabled.Value == "1";
			}
			set
			{
				if (value)
					settingVoucherEnabled.Value = "1";
				else
					settingVoucherEnabled.Value = "0";
			}
		}
		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private async void SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			if (db.BankAccounts.Count() == 0 && !AccountNumber.Text.Equals("") && !IFSCCode.Text.Equals(""))
			{
				BankAccount bankAccount = new BankAccount();
				bankAccount.AcoountNo = AccountNumber.Text;
				bankAccount.IFSCCode = IFSCCode.Text;
				bankAccount.Name = BankName.Text;
				db.BankAccounts.Add(bankAccount);
			}
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
