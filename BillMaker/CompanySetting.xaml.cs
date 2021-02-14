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

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for CompanySetting.xaml
	/// </summary>
	public partial class CompanySettingPage :INotifyPropertyChanged
	{
		private MyAttachedDbEntities db = new MyAttachedDbEntities();
		private List<MeasureUnit> measureUnits;
		CompanySetting companyName;
		CompanySetting companyPhone;
		CompanySetting companyGSTINNo;
		CompanySetting companyEmailId;
		CompanySetting companyTanNo;
		CompanySetting companyAccountNumber;
		CompanySetting comapnyIFSCCode;
		CompanySetting companyRGST;
		public event PropertyChangedEventHandler PropertyChanged;
		string mobileNumberValidation = @"^([987]{1})(\d{1})(\d{8})";

		public CompanySettingPage()
		{
			InitializeComponent();
			measureUnits = db.MeasureUnits.ToList();
			this.DataContext = this;
			companyName = db.CompanySettings.Where(x => x.Name == "CompanyName").FirstOrDefault();
			companyPhone = db.CompanySettings.Where(x => x.Name == "CompanyPhone").FirstOrDefault();
			companyGSTINNo = db.CompanySettings.Where(x => x.Name == "CompanyGSTINNo").FirstOrDefault();
			companyEmailId = db.CompanySettings.Where(x => x.Name == "CompanyEmailId").FirstOrDefault();
			companyTanNo = db.CompanySettings.Where(x => x.Name == "CompanyTANNo").FirstOrDefault();
			companyAccountNumber = db.CompanySettings.Where(x => x.Name == "CompanyAccountNumber").FirstOrDefault();
			comapnyIFSCCode = db.CompanySettings.Where(x => x.Name == "ComapnyIFSCCode").FirstOrDefault();
			companyRGST = db.CompanySettings.Where(x => x.Name == "CompanyRGST").FirstOrDefault();
		}


		public List<MeasureUnit> unitList
		{
			get
			{
				return measureUnits.Skip(1).ToList();
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

		public String CompanyAccountNumber
		{
			get
			{
				return companyAccountNumber.Value;
			}
			set
			{
				if (Regex.IsMatch(value, @"^([0-9]{11})"))
				{
					companyAccountNumber.Value = value;
				}
				else
					companyAccountNumber.Value = "";
			}
		}

		public String CompanyIFSCCode
		{
			get
			{
				return comapnyIFSCCode.Value;
			}
			set
			{
				comapnyIFSCCode.Value = value;
			}
		}

		public String CompanyRTGS
		{
			get
			{
				return companyRGST.Value;
			}
			set
			{
				companyRGST.Value = value;
			}
		}

		private void Add_Unit_Click(object sender, RoutedEventArgs e)
		{
			if(newUnitText.Text != "")
			{
				MeasureUnit measureUnit = new MeasureUnit();
				measureUnit.UnitName = newUnitText.Text;
				measureUnit.ParentId = IsBasicUnit.IsChecked.Value ? measureUnits.First().Id : (BasicUnitList.SelectedItem as MeasureUnit).Id ;
				if (!IsBasicUnit.IsChecked.Value)
					measureUnit.Conversion = (int)ConversionNumber.Value;
				else
					measureUnit.Conversion = 1;
				measureUnits.Add(measureUnit);
				db.MeasureUnits.Add(measureUnit);
				db.SaveChanges();
				Notify("unitList");
			}
		}

		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			db.SaveChanges();
			MessageBox.Show("Settings Saved Succesfully","Info",MessageBoxButton.OK);
		}

		private void IsBasicUnit_Click(object sender, RoutedEventArgs e)
		{
			bool IsBasicUnit = (e.Source as CheckBox).IsChecked.Value;
			if (!IsBasicUnit)
			{
				ConversionNumber.Value = 1;
				ConversionNumber.Visibility = Visibility.Visible;
				BasicUnitList.Visibility = Visibility.Visible;
			}
			else
			{
				ConversionNumber.Visibility = Visibility.Hidden;
				BasicUnitList.Visibility = Visibility.Hidden;
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			BasicUnitList.ItemsSource = (from tmeasureUnit in measureUnits
										 where tmeasureUnit.ParentId == measureUnits.FirstOrDefault().Id
										 select tmeasureUnit).Skip(1).ToList();
			BasicUnitList.DisplayMemberPath = "UnitName";
			BasicUnitList.SelectedValuePath = "Id";
		}
	}
}
