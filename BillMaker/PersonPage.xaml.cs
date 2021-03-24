using System;
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
using BillMaker.DataLib;
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
		BillMakerEntities db = new BillMakerEntities();
		string emailValidation = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
		string mobileNumberValidation = @"^([987]{1})(\d{1})(\d{8})";
		public Dictionary<String, String> customerVendorSelection { get; set; }
		public PersonPage()
		{
			InitializeComponent();
			_people = db.People.Where(x=>x.IsActive && x.PersonId != 1).ToList();
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
			string Title = "Error while saving";
			string MessageText = "";
			if (!IsVendorValue && !IsCustomerValue)
			{
				MessageText = "Select any one from:- \n1)Vendor \n2)Customer ";
			}
			else if(currentPerson.PersonName.Equals(""))
			{
				MessageText = "Enter Person Name";
			}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
				_ = await messageBoxDialog.ShowAsync();
				return;
			}
			if (SaveButton.Content.ToString() == "Edit Person")
			{
				updateProduct();
				SaveButton.Content = "Add Person";
			}
			else
			{
				currentPerson.AddedDate = DateTime.Now;
				currentPerson.IsActive = true;
				currentPerson = db.People.Add(currentPerson);
				db.SaveChanges();
				_people.Add(currentPerson);
			}

			if (customerVendorCombo.SelectedIndex == 0)
			{
				gridList = _people.Where(x => x.IsCustomer).OrderBy(x=>x.PersonName).ToList();
			}
			else
			{
				gridList = _people.Where(x => x.IsVendor).OrderBy(x => x.PersonName).ToList();
			}
			SearchBox.Text = "";
			gridColumns.SelectedIndex = 0;
            currentPerson = new Person();
			NotifyAll();
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
			Notify(nameof(IsCustomerValue));
			Notify(nameof(gridList));
		}
		public void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			Person personRemove;
			personRemove = (personListGrid.SelectedItem as Person);
			if (db.Sales.Where(x => x.PersonId == personRemove.PersonId).FirstOrDefault() != null)
			{
				db.People.Where(x => x.PersonId == personRemove.PersonId).FirstOrDefault().IsActive = false;
			}
			else
			{
				db.People.Remove(personRemove);
			}
			db.SaveChanges();
			_people.Remove(personRemove);
			if (customerVendorCombo.SelectedIndex == 0)
			{
				gridList = _people.Where(x => x.IsCustomer).OrderBy(x => x.PersonName).ToList();
			}
			else
			{
				gridList = _people.Where(x => x.IsVendor).OrderBy(x => x.PersonName).ToList();
			}
			Notify(nameof(gridList));

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
		}

		private void customerVendor_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (customerVendorCombo.SelectedIndex == 0)
			{
				gridList = _people.Where(x => x.IsCustomer).OrderBy(x => x.PersonName).ToList();
			}
			else
			{
				gridList = _people.Where(x => x.IsVendor).OrderBy(x => x.PersonName).ToList();
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
			updatePerson.IsCustomer = currentPerson.IsCustomer;
			updatePerson.IsVendor = currentPerson.IsVendor;
			db.SaveChanges();
		}

		private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
		{
			bool isCustomer = (customerVendorCombo.SelectedIndex == 0) ? true : false;
			if (!SearchBox.Text.Equals(""))
			{
				gridList = GlobalMethods.searchPerson(SearchBox.Text, gridColumns.SelectedValue.ToString(), _people, isCustomer).OrderBy(x => x.PersonName).ToList(); ;
				Notify(nameof(gridList));
			}
		}

        private void PersonPage_Loaded(object sender, RoutedEventArgs e)
        {
			personListGrid.Height = SystemParameters.MaximizedPrimaryScreenHeight - personListGrid.Margin.Top - GlobalMethods.MainFrameMargin - mainGrid.Margin.Bottom;
		}
	}
}
