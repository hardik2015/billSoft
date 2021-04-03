using BillMaker.DataLib;
using ModernWpf.Controls;
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
	/// Interaction logic for AccountLog.xaml
	/// </summary>
	public partial class AccountLog : INotifyPropertyChanged
	{
		BillMakerEntities DbEntities = new BillMakerEntities();
		List<Person> _people;
		List<Transaction> currentTransactions;
		public AccountLog()
		{
			_people = DbEntities.People.Where(x=>x.PersonId != 1 && x.IsActive).ToList();
			InitializeComponent();
			if(DateTime.Now.Date.Month < 4)
			{
				FromDatePicker.SelectedDate = new DateTime(DateTime.Now.AddYears(-1).Year, 4, 1);	
			}
			else
			{
				FromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, 4, 1);
			}
			ToDatePicker.SelectedDate = DateTime.Now.Date;
			this.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public List<Transaction> AccountLogDetails { get; set; }
		public Person SelectedPerson { get; set; }
		public String TotalCredited { get; set; }
		public String TotalDebited { get; set; }
		public String CurrentBalance { get; set; }
		private void CompanyAccountSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
		private void PersonSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			bool isCustomer = AccountLogSelection.SelectedIndex == 0;
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				List<Person> suggestions = _people.Where(person=>person.PersonName.Contains(PersonSearchBox.Text)).ToList();

				if (suggestions.Count > 0)
					sender.ItemsSource = suggestions;
			}
			if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
			{
				sender.Text = SelectedPerson.PersonName;
			}
		}

		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyAll()
		{
			Notify(nameof(AccountLogDetails));
			Notify(nameof(TotalDebited));
			Notify(nameof(TotalCredited));
			Notify(nameof(CurrentBalance));
		}


		private void AccountLogSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((sender as RadioButtons).SelectedIndex == 0)
			{
				PersonSearchBox.Visibility = Visibility.Hidden;
			}
			else
			{
				PersonSearchBox.Visibility = Visibility.Visible;
			}
			PaymentTypeSelection.Visibility = Visibility.Hidden;
		}

		private void PersonSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			SelectedPerson = args.SelectedItem as Person;
			PaymentTypeSelection.Visibility = Visibility.Hidden;
		}

		private void PersonSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{

		}

		private void ShowData_Click(object sender, RoutedEventArgs e)
		{
			if (AccountLogSelection.SelectedIndex == 0)
			{
				currentTransactions = DbEntities.Transactions.Where(transaction => transaction.CreatedDate >= FromDatePicker.SelectedDate.Value && transaction.CreatedDate <= ToDatePicker.SelectedDate.Value).ToList();
				AccountLogDetails = currentTransactions;
				decimal Credited = AccountLogDetails.Where(x => x.Sale.SellType).Select(x => x.Amount).Sum();
				decimal Debited = AccountLogDetails.Where(x => !x.Sale.SellType).Select(x => x.Amount).Sum();
				decimal RemainingBalance = Credited - Debited;
				TotalCredited = "Total Credited : " + Credited.ToString();
				TotalDebited = "Total Debited : " + Debited.ToString();
				CurrentBalance = "Total change in Balance : " + RemainingBalance.ToString();
				NotifyAll();
			}
			else
			{
				currentTransactions = DbEntities.Transactions.Where(transaction => transaction.Sale.PersonId == SelectedPerson.PersonId && transaction.CreatedDate >= FromDatePicker.SelectedDate.Value && transaction.CreatedDate <= ToDatePicker.SelectedDate.Value).ToList();
				AccountLogDetails = currentTransactions;
				decimal Credited = AccountLogDetails.Where(x => x.Sale.SellType).Select(x => x.Amount).Sum();
				decimal Debited = AccountLogDetails.Where(x => !x.Sale.SellType).Select(x => x.Amount).Sum();
				decimal RemainingBalance = Credited - Debited;
				TotalCredited = "Total Credited : " + Credited.ToString();
				TotalDebited = "Total Debited : " + Debited.ToString();
				CurrentBalance = "Total change in Balance : " + RemainingBalance.ToString();
				NotifyAll();
			}
			PaymentTypeSelection.Visibility = Visibility.Visible;
		}

		private void PaymentTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (AccountLogDetails != null)
			{
				RadioButtons radioButtons = sender as RadioButtons;
				AccountLogDetails = currentTransactions.Where(x => x.PaymentType == radioButtons.SelectedIndex+1).ToList();
				decimal Credited = AccountLogDetails.Where(x => x.Sale.SellType).Select(x => x.Amount).Sum();
				decimal Debited = AccountLogDetails.Where(x => !x.Sale.SellType).Select(x => x.Amount).Sum();
				decimal RemainingBalance = Credited - Debited;
				TotalCredited = "Total Credited : " + Credited.ToString();
				TotalDebited = "Total Debited : " + Debited.ToString();
				CurrentBalance = "Total change in Balance : " + RemainingBalance.ToString();
				NotifyAll();
			}
		}
	}
	public class BoolToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			SolidColorBrush brush = new SolidColorBrush(Colors.LimeGreen);
			bool booleanValue = true;
			Boolean.TryParse(value.ToString(), out booleanValue);

			if (booleanValue)
				brush = new SolidColorBrush(Colors.LightGreen);
			else
				brush = new SolidColorBrush(Colors.Red);

			return brush;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IntToPaymentTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int intValue = 0;
			Int32.TryParse(value.ToString(), out intValue);

			if(intValue == 1)
			{
				return "Cash";
			}
			if (intValue == 2)
			{
				return "Check";
			}
			if (intValue == 3)
			{
				return "Payment Panding";
			}
			return "";

		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
