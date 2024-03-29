﻿using System;
using System.Collections.Generic;
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
using BillMaker.DataConnection;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for PersonPage.xaml
	/// </summary>
	public partial class PersonPage : INotifyPropertyChanged
	{
		List<Person> _people;
		Person currentPerson = new Person();
		MyAttachedDbEntities db = new MyAttachedDbEntities();
		string emailValidation = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
		string mobileNumberValidation = @"^([987]{1})(\d{1})(\d{8})";
		public Dictionary<String, String> customerVendorSelection { get; set; }
		public PersonPage()
		{
			InitializeComponent();
			_people = db.People.ToList();
			currentPerson = new Person();
			this.DataContext = this;
			customerVendorSelection = new Dictionary<string, string>();
			customerVendorSelection.Add("CustomerList", "Show Customer List");
			customerVendorSelection.Add("VendorList", "Show Vendor List");
		}

		/// <summary>
		/// Declare Binding Properties
		/// </summary>
		#region Properties
		public string NameValue
		{
			get
			{
				return currentPerson.PersonName;
			}
			set
			{
				currentPerson.PersonName = value;
			}
		}

		public string EmailValue
		{
			get
			{
				return currentPerson.Email;
			}
			set
			{
				if (value.Length == 0 || Regex.IsMatch(value, emailValidation))
				{
					currentPerson.Email = value;
				}
			}
		}

		public string PhoneValue
		{
			get
			{
				return currentPerson.Phone;
			}
			set
			{
				if (Regex.IsMatch(value, mobileNumberValidation))
				{
					currentPerson.Phone = value;
				}
				else
					currentPerson.Phone = "";
			}
		}

		public string AddressValue
		{
			get
			{
				return currentPerson.Address;
			}
			set
			{
				currentPerson.Address = value;
			}
		}

		public string CityValue
		{
			get
			{
				return currentPerson.City;
			}
			set
			{
				currentPerson.City = value;
			}
		}

		public string StateValue
		{
			get
			{
				return currentPerson.State;
			}
			set
			{
				currentPerson.State = value;
			}
		}

		public string CountryValue
		{
			get
			{
				return currentPerson.Country;
			}
			set
			{
				currentPerson.Country = value;
			}
		}

		public bool IsVendorValue
		{
			get
			{
				return currentPerson.IsVendor;
			}
			set
			{
				currentPerson.IsVendor = value;
			}
		}

		public bool IsCustomerValue
		{
			get
			{
				return currentPerson.IsCustomer;
			}
			set
			{
				currentPerson.IsCustomer = value;
			}
		}

		public List<Person> gridList
		{
			get;
			set;
		}

		#endregion 


		public event PropertyChangedEventHandler PropertyChanged;

		private async void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (!IsVendorValue && !IsCustomerValue)
			{
				string Title = "Error while saving";
				string MessageText = "Select any one from:- \n1)Product \n2)Raw Material ";
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title,MessageText);
				_ = await messageBoxDialog.ShowAsync();
				return;
			}
			if (SaveButton.Content.ToString() == "Edit Person")
			{
				updateProduct();
			}
			else
			{
				currentPerson.AddedDate = DateTime.Now;
				currentPerson = db.People.Add(currentPerson);
				db.SaveChanges();
				_people.Add(currentPerson);
				currentPerson = new Person();
				NotifyAll();
			}
		}

		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyAll()
		{
			Notify(nameof(NameValue));
			Notify(nameof(EmailValue));
			Notify(nameof(PhoneValue));
			Notify(nameof(AddressValue));
			Notify(nameof(CityValue));
			Notify(nameof(StateValue));
			Notify(nameof(CountryValue));
			Notify(nameof(IsVendorValue));
			Notify(nameof(IsCustomerValue));
			Notify(nameof(gridList));
		}
		public void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			Person personRemove;
			personRemove = (personListGrid.SelectedItem as Person);
			db.People.Remove(personRemove);
			db.SaveChanges();
			_people.Remove(personRemove);
		}
		public void btnEdit_Click(object sender, RoutedEventArgs e)
		{
			SaveButton.Content = "Edit Person";
			Button edit = sender as Button;
			currentPerson = personListGrid.SelectedItem as Person;
			NotifyAll();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			//PersonTab.Items.Add(Customer);
			//PersonTab.Items.Add(Vendor);
		}

		private void customerVendor_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (customerVendorCombo.SelectedValue.ToString() == "CustomerList")
			{
				gridList = _people.Where(x => x.IsCustomer).ToList();
			}
			else
			{
				gridList = _people.Where(x => x.IsVendor).ToList();
			}
			Notify(nameof(gridList));
		}

		private void updateProduct()
		{
			Person updatePerson = db.People.Where(x => x.PersonId == currentPerson.PersonId).FirstOrDefault();
			updatePerson.PersonName = currentPerson.PersonName;
			updatePerson.Email = currentPerson.Email;
			updatePerson.Phone = currentPerson.Phone;
			updatePerson.Address = currentPerson.Address;
			updatePerson.City = currentPerson.City;
			updatePerson.State = currentPerson.State;
			updatePerson.Country = currentPerson.Country;
			updatePerson.IsCustomer = currentPerson.IsCustomer;
			updatePerson.IsVendor = currentPerson.IsVendor;
			db.SaveChanges();
		}

		private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
		{
			bool isVendor = (customerVendorCombo.SelectedValue.ToString() == "VendorList") ? false : true;
			if (!SearchBox.Text.Equals(""))
			{
				gridList = GlobalMethods.searchPerson(SearchBox.Text, gridColumns.SelectedValue.ToString(), _people, isVendor);
				Notify(nameof(gridList));
			}
		}

	}
}
