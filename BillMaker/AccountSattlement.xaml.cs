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
	/// Interaction logic for AccountSattlement.xaml
	/// </summary>
	public partial class AccountSattlement : INotifyPropertyChanged
	{
		BillMakerEntities DbEntities = new BillMakerEntities();
		List<Person> _people;

		public event PropertyChangedEventHandler PropertyChanged;

		public AccountSattlement()
		{
			this.DataContext = this;
			_people = DbEntities.People.Where(x=>x.PersonId != 1 && x.IsActive).ToList();
			InitializeComponent();
			SettlementDate.SelectedDate = DateTime.Now.Date;
		}

		public List<PandingPaymentDetails> pandingDetails { get;set; }
		public Transaction CurrentTransaction { get; set; }
		public Person SelectedPerson { get; set; }
		public PandingPaymentDetails CurentSettlementSale { get; set; }
		private void PersonSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			bool isCustomer = SaleTypeSelection.SelectedIndex == 0;
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				List<Person> suggestions = GlobalMethods.searchPerson(sender.Text, "Name", _people, isCustomer).Take(5).ToList(); ;

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
		}


		private void SaleTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((sender as RadioButtons).SelectedIndex == 0)
			{
				PersonSearchBox.Header = "Customer";
			}
			else
			{
				PersonSearchBox.Header = "Vendor";
			}
		}

		private void PersonSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			SelectedPerson = args.SelectedItem as Person;
			pandingDetails = new List<PandingPaymentDetails>();
			Notify(nameof(pandingDetails));
		}

		private void PersonSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{

		}
		
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			List<Sale> sales = DbEntities.Sales.Where(sale => sale.PersonId == SelectedPerson.PersonId).ToList();
			sales = sales.Where(sale => sale.Transactions.Where(tranasction => tranasction.PaymentType == 3).Count() > 0).ToList();
			List<PandingPaymentDetails> details = new List<PandingPaymentDetails>();
			foreach (Sale sale in sales)
			{
				PandingPaymentDetails panding = new PandingPaymentDetails();
				panding.sale = sale;
				panding.PandingPayment = sale.Transactions.Where(x => x.PaymentType == 3).First().Amount;
				details.Add(panding);
			}
			pandingDetails = details.ToList();
			Notify(nameof(pandingDetails));
		}

		public class PandingPaymentDetails
		{
			public Sale sale { get; set; }
			public Decimal PandingPayment { get; set; }
			public PandingPaymentDetails()
			{

			}
		}

		private void btnSettle_Click(object sender, RoutedEventArgs e)
		{
			PaymentButtonGrid.Visibility = Visibility.Visible;
			CurentSettlementSale = paymentPendingGrid.SelectedItem as PandingPaymentDetails;
		}

		private void PaymentButton_Click(object sender, RoutedEventArgs e)
		{
			Button paymentButton = sender as Button;
			if(PaymentGrid.Visibility == Visibility.Visible)
			{
				return;
			}
			PaymentGrid.Visibility = Visibility.Visible;
			AmountBox.Value = (double)CurentSettlementSale.PandingPayment;
			if (paymentButton.Name == "CashButton")
			{
				CurrentTransaction = new Transaction();
				CurrentTransaction.PaymentType = 1;
			}
			else if(paymentButton.Name == "CheckButton")
			{
				CurrentTransaction = new Transaction();
				CurrentTransaction.PaymentType = 2;
				CheckNumberBox.Visibility = Visibility.Visible;
			}
			/*else if(paymentButton.Name == "RedeemtionButton")
			{
				PaymentGrid.Visibility = Visibility.Hidden;
			}*/
		}

		private async void PaymentDoneButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurentSettlementSale.PandingPayment >= (decimal)AmountBox.Value)
			{
				Sale sale = DbEntities.Sales.Where(x => x.Id == CurentSettlementSale.sale.Id).FirstOrDefault();
				Transaction transaction = sale.Transactions.Where(trans => trans.PaymentType == 3).FirstOrDefault();
				DbEntities.Transactions.Remove(transaction);
				CurrentTransaction.CreatedDate = SettlementDate.SelectedDate.Value.Date;
				CurrentTransaction.Amount = (decimal)AmountBox.Value;
				if(CurrentTransaction.PaymentType == 2)
				{
					BankAccount account = DbEntities.BankAccounts.Where(bank => bank.Id == 1).FirstOrDefault();
					if(account == null)
					{
						MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error !!", "Add Atleast one Bank details");
						_ = await messageBoxDialog.ShowAsync();
						return;
					}
					if (sale.SellType)
					{
						account.Balance += CurrentTransaction.Amount;
					}
					else
					{
						account.Balance -= CurrentTransaction.Amount;
					}
					TransactionProperty check = new TransactionProperty() { Transaction = CurrentTransaction, PropertyName = account.AcoountNo, PropertyValue = CheckNumberBox.Text };
					CurrentTransaction.TransactionProperties.Add(check);
				}
				sale.Transactions.Add(CurrentTransaction);
				DbEntities.SaveChanges();
				decimal remainingPayment = transaction.Amount - CurrentTransaction.Amount;
				Transaction transaction1 = null;
				if (remainingPayment > 0)
				{
					transaction1 = new Transaction();
					transaction1.PaymentType = 3;
					transaction1.Amount = remainingPayment;
					transaction1.CreatedDate = DateTime.Now.Date;
					transaction1.Sale = sale;
					sale.Transactions.Add(transaction1);
				}
				if(sale.SellType)
				{
					SelectedPerson.Account += CurrentTransaction.Amount;
				}
				else
				{
					SelectedPerson.Account -= CurrentTransaction.Amount;
				}
				DbEntities.SaveChanges();
				if (transaction1 == null)
				{
					List<PandingPaymentDetails> paymentDetails = pandingDetails;
					paymentDetails.Remove(CurentSettlementSale);
					pandingDetails = paymentDetails.ToList();
					Notify(nameof(pandingDetails));
				}
				else
				{
					List<PandingPaymentDetails> paymentDetails = pandingDetails;
					paymentDetails.Remove(CurentSettlementSale);
					CurentSettlementSale.PandingPayment = transaction1.Amount;
					CurentSettlementSale.sale = sale;
					paymentDetails.Add(CurentSettlementSale);
					pandingDetails = paymentDetails.ToList();
					Notify(nameof(pandingDetails));
				}
				PaymentGrid.Visibility = Visibility.Hidden;
				CurrentTransaction = null;
			}
		}

		private void PaymentCancelButton_Click(object sender, RoutedEventArgs e)
		{
			CurrentTransaction = null;
			PaymentGrid.Visibility = Visibility.Hidden;
		}
	}
}
