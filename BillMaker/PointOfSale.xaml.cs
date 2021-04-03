using BillMaker.DataLib;
using log4net;
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
	/// Interaction logic for PointOfSale.xaml
	/// </summary>
	public partial class PointOfSale : INotifyPropertyChanged
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		BillMakerEntities db = new BillMakerEntities();
		List<Product> _products;
		List<Person> _people;
		List<ProductUnit> _productUnits;
		List<Voucher> _currentVouchers;
		Sale currentSale;
		Decimal _totalSalePrice;
		Decimal _totalCgstTax;
		Decimal _totalSgstTax;
		Decimal _totalAmountToPaid;
		Decimal _paidViaCheck;
		Decimal _paidViaCash;
		Decimal _paidViaAccount;
		int _currentPaymentType = 0;
		bool IsVoucherRedeemtion;
		public event PropertyChangedEventHandler PropertyChanged;

		public PointOfSale()
		{
			log4net.Config.XmlConfigurator.Configure();
			try
			{
				log.Info("Start Processing Point Of Sale");
				IsVoucherRedeemtion = false;
				GlobalMethods.LoadCompanyDetails();
				_products = db.Products.Where(x => x.IsActive).ToList();
				_people = db.People.Where(x => x.IsActive && x.PersonId != 1).ToList();
				_productUnits = db.ProductUnits.Where(x => x.IsActive).ToList();
				_totalSalePrice = _totalAmountToPaid = _totalCgstTax = _totalSgstTax = (Decimal)0.00;
				InitializeComponent();
				this.DataContext = this;
				SaleDateTime.SelectedDate = DateTime.Now;
				log.Info("End Loading Point Of Sale");

			}
			catch (Exception e)
			{
				log.Error("Exception : " + e.ToString());
			}
		}

		public Product SelectedProduct
		{
			get; set;
		}
		public Person SelectedPerson
		{
			get; set;
		}

		public decimal TotalMRP
		{
			get; set;
		}
		public String TotalAmountToPaid
		{
			get
			{
				return decimal.Round(_totalAmountToPaid, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public String TotalCgstTax
		{
			get
			{
				return decimal.Round(_totalCgstTax, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public String TotalSgstTax
		{
			get
			{
				return decimal.Round(_totalSgstTax, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public String TotalSalePrice
		{
			get
			{
				return decimal.Round(_totalSalePrice, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public String TotalPaidAmount
		{
			get
			{
				return decimal.Round(_paidViaCash + _paidViaCheck + _paidViaAccount, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public String RemainingPayment
		{
			get
			{
				return decimal.Round(_totalAmountToPaid - _paidViaCheck - _paidViaCash - _paidViaAccount, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public List<order_details> OrderItemList
		{
			get
			{
				if (currentSale == null)
					return new List<order_details>();
				else
					return currentSale.order_details.ToList();
			}
		}

		private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			bool isProduct = SaleTypeSelection.SelectedIndex == 0;
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				if (sender.Text != "")
				{
					var suggestions = GlobalMethods.searchProduct(sender.Text, "Name", _products, isProduct).Take(5).ToList();

					if (suggestions.Count > 0)
						sender.ItemsSource = suggestions;

				}
				else
				{
					Unit.IsEnabled = false;
					Quantity.IsEnabled = false;
					TotalMRPBox.IsEnabled = false;
				}
			}
			if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
			{
				sender.Text = SelectedProduct.Name;
			}
		}

		private void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{

		}

		private async void Search_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			SelectedProduct = args.SelectedItem as Product;
			String messageText = "";
			if (SelectedProduct.ProductUnits.ToList().Count == 0)
			{
				messageText = "Please add unit for this product from Unit Configuration Section.";
			}
			if (SelectedProduct.IsUnitsConnected && SelectedProduct.ProductUnits.Where(x => x.IsBasicUnit).ToList().Count == 0)
			{
				messageText = "Please add Basic unit for this product from Unit Configuration Section.";
			}
			if (!messageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error!!", messageText);
				_ = await messageBoxDialog.ShowAsync();
				SelectedProduct = null;
				sender.Text = "";
				return;
			}
			if (GlobalMethods.IsVoucherEnabled && CreateOrderDetailsForVouchers())
			{
				NotifyAll();
			}
			Unit.IsEnabled = true;
			Quantity.IsEnabled = true;
			TotalMRPBox.IsEnabled = true;
			List<ProductUnit> productUnits = _productUnits.Where(x => x.Product == SelectedProduct).ToList();
			Unit.ItemsSource = productUnits;
			Unit.SelectedIndex = 0;
			TotalMRP = currentSale.SellType ? productUnits[0].UnitSellPrice : productUnits[0].UnitBuyPrice;
			Notify(nameof(TotalMRP));
			ChangeProductBtn.Visibility = Visibility.Visible;
		}

		private bool CreateOrderDetailsForVouchers()
		{
			_currentVouchers = db.Vouchers.Where(voucher => voucher.ProductId == SelectedProduct.Id && voucher.VoucherType == currentSale.SellType && voucher.PersonId == SelectedPerson.PersonId).ToList();
			foreach (Voucher voucher in _currentVouchers)
			{
				order_details order = new order_details();
				order.Product = voucher.Product;
				order.ProductUnit = voucher.ProductUnit;
				order.Quantity = voucher.ValueAdded;
				order.TotalPrice = (voucher.VoucherType ?  voucher.ProductUnit.UnitSellPrice : voucher.ProductUnit.UnitBuyPrice )*order.Quantity;
				currentSale.order_details.Add(order);
				order.Sale = currentSale;
				order.Voucher = voucher;
				order.Calculate();
				_totalSalePrice += order.TotalTaxCalculatedPrice;
				_totalAmountToPaid += order.TotalPrice;
				_totalCgstTax += order.TotalCgstPrice;
				_totalSgstTax += order.TotalSgstPrice;
				currentSale.order_details.Add(order);
			}
			if (_currentVouchers.Count > 0)
				IsVoucherRedeemtion = true;

			return IsVoucherRedeemtion;
		}

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

		private void PersonSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			SelectedPerson = args.SelectedItem as Person;
			PersonSearchBox.IsEnabled = false;
			SaleTypeSelection.IsEnabled = false;
			ItemSearchBox.IsEnabled = true;
			currentSale = new Sale();
			currentSale.Person = SelectedPerson;
			currentSale.SellType = SaleTypeSelection.SelectedIndex == 0;
			currentSale.CreatedDate = DateTime.Now;
			CancelSale.Visibility = Visibility.Visible;
			AddClientBtn.Visibility = Visibility.Hidden;
		}


		private void PersonSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{

		}

		private async void AddItem_Click(object sender, RoutedEventArgs e)
		{
			string Title = "Error while saving"; ;
			string MessageText = "";
			bool isSecoundryButtonEnabled = false;
			if (SelectedProduct == null)
			{
				MessageText = "Please select an product";
			}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText, isSecoundryButtonEnabled);
				ContentDialogResult result = await messageBoxDialog.ShowAsync();
				if (result == ContentDialogResult.Primary)
					return;
			}
			order_details order = new order_details();
			order.Product = SelectedProduct;
			order.Quantity = Decimal.Parse(Quantity.Value.ToString(".00"));
			order.ProductUnit = Unit.SelectedItem as ProductUnit;
			order.TotalPrice = Decimal.Parse(TotalMRPBox.Value.ToString(".00"));
			if (SelectedProduct != null && currentSale.SellType)
				if (SelectedProduct.IsUnitsConnected)
				{
					Title = "Warning";
					ProductUnit basicUnit = SelectedProduct.ProductUnits.Where(x => x.IsBasicUnit).FirstOrDefault();
					if (basicUnit == null)
					{
						Title = "Error";
						MessageText = "Product units are connected and no basic unit is set so do that";
					}
					else
					{
						decimal StockDecressed = order.ProductUnit.Conversion * order.Quantity;
						if (basicUnit.Stock < StockDecressed)
						{
							MessageText = "Your are out of stock for place this order.";
							isSecoundryButtonEnabled = true;
						}
					}
				}
				else
				{
					if (order.ProductUnit.Stock < order.Quantity)
					{
						MessageText = "Your are out of stock for place this order.";
						isSecoundryButtonEnabled = true;
					}
				}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText, isSecoundryButtonEnabled);
				ContentDialogResult result = await messageBoxDialog.ShowAsync();
				if (result == ContentDialogResult.Secondary)
					return;
			}

			currentSale.order_details.Add(order);
			order.Sale = currentSale;
			order.Calculate();
			_totalSalePrice += order.TotalTaxCalculatedPrice;
			_totalAmountToPaid += order.TotalPrice;
			_totalCgstTax += order.TotalCgstPrice;
			_totalSgstTax += order.TotalSgstPrice;
			ItemSearchBox.Text = "";
			Quantity.Value = 1;
			TotalMRP = 0;
			Unit.ItemsSource = null;
			SelectedProduct = null;
			TotalMRPBox.IsEnabled = false;
			Unit.IsEnabled = false;
			Quantity.IsEnabled = false;
			ItemSearchBox.Focus();
			ChangeProductBtn.Visibility = Visibility.Hidden;
			NotifyAll();
		}

		public void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			order_details order = OrderItems.SelectedItem as order_details;
			currentSale.order_details.Remove(order);
			_totalSalePrice -= order.TotalTaxCalculatedPrice;
			_totalAmountToPaid -= order.TotalPrice;
			_totalCgstTax -= order.TotalCgstPrice;
			_totalSgstTax -= order.TotalSgstPrice;
			if(_currentVouchers != null  && order.Voucher != null)
				_currentVouchers.Remove(order.Voucher);
			NotifyAll();
		}

		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyAll()
		{
			Notify(nameof(TotalAmountToPaid));
			Notify(nameof(TotalCgstTax));
			Notify(nameof(TotalSgstTax));
			Notify(nameof(TotalSalePrice));
			Notify(nameof(OrderItemList));
			Notify(nameof(TotalPaidAmount));
			Notify(nameof(RemainingPayment));
			Notify(nameof(TotalMRP));
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			NotifyAll();
			int count = 1;
			OrderItems.Height = 0;
			foreach (RowDefinition row in MainGrid.RowDefinitions)
			{
				OrderItems.Height += row.ActualHeight;
				if (count == 7)
					break;
				else
					count++;
			}
			OrderItems.Width = OrderItems.ActualWidth;
		}

		private void PaymentButton_Click(object sender, RoutedEventArgs e)
		{
			if (PaymentGrid.Visibility == Visibility.Visible || currentSale == null || currentSale.order_details.Count <= 0)
				return;
			Button paymentButton = sender as Button;
			if (paymentButton.Name == CheckButton.Name)
			{
				CheckNumberBox.Visibility = Visibility.Visible;
				AmountBox.Value = (double)(_totalAmountToPaid - _paidViaCash - _paidViaAccount);
				_paidViaCheck = 0;
				_currentPaymentType = 2;
			}
			else if(paymentButton == AccountButton)
			{
				AmountBox.Value = (double)(_totalAmountToPaid - _paidViaCash - _paidViaCheck);
				_paidViaAccount = 0;
				_currentPaymentType = 3;
			}
			else
			{
				AmountBox.Value = (double)(_totalAmountToPaid - _paidViaCheck - _paidViaAccount);
				_paidViaCash = 0;
				_currentPaymentType = 1;
			}
			PaymentGrid.Visibility = Visibility.Visible;
		}

		private async void PaymentDoneButton_Click(object sender, RoutedEventArgs e)
		{
			decimal AmountValue = (decimal)AmountBox.Value;
			if (AmountValue != 0)
			{
				if (_totalAmountToPaid - _paidViaCash - _paidViaCheck - (decimal)AmountBox.Value < 0)
				{
					MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error!!", "Paying more than customer need to pay");
					_ = await messageBoxDialog.ShowAsync();
					return;
				}
				Transaction transaction = new Transaction
				{
					Amount = AmountValue,
					CreatedDate = SaleDateTime.SelectedDate.Value.Date
				};
				if (_currentPaymentType == 2)
				{
					TransactionProperty transactionProperty = new TransactionProperty
					{
						PropertyName = "",
						PropertyValue = CheckNumberBox.Text
					};
					transaction.PaymentType = 2;
					transactionProperty.PropertyName = db.BankAccounts.Where(account => account.Id == 1).FirstOrDefault().Id.ToString();
					transaction.TransactionProperties.Add(transactionProperty);
					_paidViaCheck += AmountValue;
				}
				else if(_currentPaymentType == 3)
				{
					transaction.PaymentType = 3;
					_paidViaAccount += AmountValue;
				}
				else if(_currentPaymentType == 1)
				{
					transaction.PaymentType = 1;
					_paidViaCash += AmountValue;
				}

				currentSale.Transactions.Add(transaction);

			}
			PaymentGrid.Visibility = Visibility.Hidden;
			CheckNumberBox.Visibility = Visibility.Hidden;
			_currentPaymentType = 0;
			NotifyAll();
		}

		private void CancelPayment_Click(object sender, RoutedEventArgs e)
		{
			CheckNumberBox.Visibility = Visibility.Hidden;
			PaymentGrid.Visibility = Visibility.Hidden;
			_currentPaymentType = 0;
		}

		private async void FinishSale_Click(object sender, RoutedEventArgs e)
		{

			if (currentSale == null)
				return;
			currentSale.CreatedDate = SaleDateTime.SelectedDate.Value.Date;
			if (SaleDateTime.SelectedDate.Value.Date == DateTime.Now.Date)
			{
				currentSale.CreatedDate = DateTime.Now.Date;
			}
			string Title = "Error while saving"; ;
			string MessageText = "";
			if (_totalAmountToPaid > _paidViaCheck + _paidViaCash + _paidViaAccount)
			{
				MessageText = "Please do the full Payment";
			}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
				_ = await messageBoxDialog.ShowAsync();
				return;
			}
			bool isSell = SaleTypeSelection.SelectedIndex == 0;
			if (!IsVoucherRedeemtion)
				foreach (order_details details in currentSale.order_details)
				{
					ProductUnit productUnit;
					StockLog stockLog = new StockLog();
					if (details.Product.IsUnitsConnected)
					{
						productUnit = details.Product.ProductUnits.Where(x => x.IsBasicUnit).FirstOrDefault();
						if(isSell)
							productUnit.Stock -= details.Quantity * details.ProductUnit.Conversion;
						else
							productUnit.Stock += details.Quantity * details.ProductUnit.Conversion;
					}
					else
					{
						if(isSell)
							details.ProductUnit.Stock -= details.Quantity;
						else
							details.ProductUnit.Stock += details.Quantity;
					}
					stockLog.ProductUnit = details.ProductUnit;
					if(isSell)
						stockLog.AddedValue = -details.Quantity;
					else
						stockLog.AddedValue = details.Quantity;
					stockLog.AddedDate = currentSale.CreatedDate;
					db.StockLogs.Add(stockLog);
				}
			currentSale = db.Sales.Add(currentSale);
			db.SaveChanges();
			if(_paidViaAccount > 0)
			{
				if(isSell)
					db.People.Where(person => person.PersonId == SelectedPerson.PersonId).FirstOrDefault().Account -= Decimal.Round(_paidViaAccount, 2, MidpointRounding.AwayFromZero);
				else
					db.People.Where(person => person.PersonId == SelectedPerson.PersonId).FirstOrDefault().Account += Decimal.Round( _paidViaAccount,2 ,MidpointRounding.AwayFromZero); 
			}
			if(_paidViaCheck > 0)
			{
				if (isSell)
					db.BankAccounts.Where(account => account.Id == 1).FirstOrDefault().Balance += Decimal.Round(_paidViaAccount, 2, MidpointRounding.AwayFromZero);
				else
					db.BankAccounts.Where(account => account.Id == 1).FirstOrDefault().Balance -= Decimal.Round(_paidViaAccount, 2, MidpointRounding.AwayFromZero);

			}
			if (_currentVouchers != null)
			{
				foreach (Voucher voucher in _currentVouchers)
				{
					db.Vouchers.Remove(voucher);
				}
			}
			db.SaveChanges();
			SaleDetails billReciept = new SaleDetails(currentSale);
			Frame.Navigate(billReciept);
			NewSaleStarted();
		}

		private void Quantity_LostFocus(object sender, RoutedEventArgs e)
		{
			NumberBox numberBox = sender as NumberBox;
			ProductUnit measureUnit = Unit.SelectedItem as ProductUnit;
			decimal UnitPrice = measureUnit.UnitSellPrice;
			TotalMRP = decimal.Round((UnitPrice * (decimal)numberBox.Value), 2, MidpointRounding.AwayFromZero);
			Notify(nameof(TotalMRP));
		}

		private void Unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ProductUnit measureUnit = (sender as ComboBox).SelectedItem as ProductUnit;
			if (measureUnit == null)
				return;
			decimal UnitPrice = currentSale.SellType ? measureUnit.UnitSellPrice : measureUnit.UnitBuyPrice;
			TotalMRP = decimal.Round((UnitPrice * (decimal)Quantity.Value), 2, MidpointRounding.AwayFromZero);
			Notify(nameof(TotalMRP));
		}

		private void CancelSale_Click(object sender, RoutedEventArgs e)
		{
			NewSaleStarted();
		}

		private void NewSaleStarted()
		{
			currentSale = null;
			_totalSalePrice = 0;
			_totalAmountToPaid = 0;
			_totalCgstTax = 0;
			_totalSgstTax = 0;
			_currentVouchers = null;
			ItemSearchBox.Text = "";
			Quantity.Value = 1;
			TotalMRP = 0;
			Unit.ItemsSource = null;
			SelectedProduct = null;
			TotalMRPBox.IsEnabled = false;
			Unit.IsEnabled = false;
			Quantity.IsEnabled = false;
			PersonSearchBox.IsEnabled = true; ;
			ItemSearchBox.IsEnabled = false;
			PersonSearchBox.Text = "";
			SaleDateTime.DisplayDate = DateTime.Now;
			AddClientBtn.Visibility = Visibility.Visible;
			AccountButton.Visibility = Visibility.Visible;
			NotifyAll();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ItemSearchBox.IsEnabled = true;
			Quantity.Value = 1;
			TotalMRP = 0;
			Unit.ItemsSource = null;
			SelectedProduct = null;
			TotalMRPBox.IsEnabled = false;
			Unit.IsEnabled = false;
			Quantity.IsEnabled = false;
			ItemSearchBox.Text = "";
			ItemSearchBox.Focus();
			Notify(nameof(TotalMRP));
			ChangeProductBtn.Visibility = Visibility.Hidden;
		}

		private async void AddQuickClient_Click(object sender, RoutedEventArgs e)
		{

			String MessageText = "";
			if (PersonSearchBox.Text.Equals(""))
			{
				MessageText = "Add name of the new client in Client Searchbox";
			}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error !!", MessageText);
				_ = await messageBoxDialog.ShowAsync();
				PersonSearchBox.Focus();
				return;
			}
			SelectedPerson = _people.Where(x=>x.PersonId == 1).FirstOrDefault();
			PersonSearchBox.IsEnabled = false;
			ItemSearchBox.IsEnabled = true;
			currentSale = new Sale();
			currentSale.Person = SelectedPerson;
			currentSale.SellType = true;
			currentSale.PersonName = PersonSearchBox.Text;
			currentSale.CreatedDate = DateTime.Now;
			CancelSale.Visibility = Visibility.Visible;
			AddClientBtn.Visibility = Visibility.Hidden;
			AccountButton.Visibility = Visibility.Hidden;
		}

		private void SaleTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if((sender as RadioButtons).SelectedIndex == 0)
			{
				PersonSearchBox.Header = "Customer";
				ItemSearchBox.Header = "Product";
			}
			else
			{
				PersonSearchBox.Header = "Vendor";
				ItemSearchBox.Header = "Raw Material";
			}
		}
	}
}
