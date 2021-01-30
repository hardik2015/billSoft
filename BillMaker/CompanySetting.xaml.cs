using BillMaker.DataConnection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
		CompanySetting companyHSNNo;
		CompanySetting companyEmailId;
		public event PropertyChangedEventHandler PropertyChanged;

		public CompanySettingPage()
		{
			InitializeComponent();
			measureUnits = db.MeasureUnits.ToList();
			this.DataContext = this;
			companyName = db.CompanySettings.Where(x => x.Name == "CompanyName").FirstOrDefault();
			companyPhone = db.CompanySettings.Where(x => x.Name == "CompanyPhone").FirstOrDefault();
			companyHSNNo = db.CompanySettings.Where(x => x.Name == "HSNNo").FirstOrDefault();
			companyEmailId = db.CompanySettings.Where(x => x.Name == "CompanyEmailId").FirstOrDefault();
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
				companyName.Value = companyNameText.Text;
			}
		}

		public Int32 CompanyPhone
		{
			get
			{
				return companyPhone.Value.CompareTo("") == 0  ? 00 : Int32.Parse(companyPhone.Value);
			}
			set
			{
				companyPhone.Value = phoneNoText.Value.ToString();
			}
		}
		
		public String CompanyHSNNo
		{
			get
			{
				return companyHSNNo.Value;
			}
			set
			{
				companyHSNNo.Value = hsnNoText.Text;
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
				companyEmailId.Value = emailIdText.Text;
			}
		}

		private void Add_Unit_Click(object sender, RoutedEventArgs e)
		{
			if(newUnitText.Text != "")
			{
				MeasureUnit measureUnit = new MeasureUnit();
				measureUnit.UnitName = newUnitText.Text;
				measureUnit.ParentId = IsBasicUnit.IsChecked.Value ? measureUnits.First().Id : (BasicUnitList.SelectedItem as MeasureUnit).Id ;
				if(!IsBasicUnit.IsChecked.Value)
					measureUnit.Conversion = (int)ConversionNumber.Value;
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
