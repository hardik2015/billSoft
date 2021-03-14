using BillMaker.DataConnection;
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
		MyAttachedDbEntities db = new MyAttachedDbEntities();
		List<Product> _products;
		List<Person> _people;
		List<ProductUnit> _productUnits;
		Sale currentSale;
		Decimal _totalSalePrice;
		Decimal _totalCgstTax;
		Decimal _totalSgstTax;
		Decimal _totalAmountToPaid;
		Decimal _paidViaCheck;
		Decimal _paidViaCash;

		public event PropertyChangedEventHandler PropertyChanged;

		public PointOfSale()
		{
			log4net.Config.XmlConfigurator.Configure();
			try
			{
				log.Info("Start Processing Point Of Sale");
				_products = db.Products.Where(x=>x.IsActive).ToList();
				_people = db.People.Where(x => x.IsActive).ToList();
				_productUnits = db.ProductUnits.Where(x=>x.IsActive).ToList();
				_totalSalePrice = _totalAmountToPaid = _totalCgstTax = _totalSgstTax = (Decimal)0.00;
				InitializeComponent();
				this.DataContext = this;
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
			get;set;
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
				return decimal.Round(_paidViaCash + _paidViaCheck, 2, MidpointRounding.AwayFromZero).ToString();
			}
		}

		public String RemainingPayment
		{
			get
			{
				return decimal.Round(_totalAmountToPaid - _paidViaCheck - _paidViaCash, 2, MidpointRounding.AwayFromZero).ToString();
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

		private void SearchBox_TextChanged(AutoSuggestBox sender ,AutoSuggestBoxTextChangedEventArgs args)
		{
			bool isProduct = SaleTypeSelection.SelectedIndex == 0 ? true : false;
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
			if(args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
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
			if (SelectedProduct.IsUnitsConnected && SelectedProduct.ProductUnits.Where(x=>x.IsBasicUnit).ToList().Count == 0)
            {
				messageText = "Please add Basic unit for this product from Unit Configuration Section.";
			}
			if(!messageText.Equals(""))
            {
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error!!", messageText);
				_ = await messageBoxDialog.ShowAsync();
				SelectedProduct = null;
				sender.Text = "";
				return;
			}
			Unit.IsEnabled = true;
			Quantity.IsEnabled = true;
			TotalMRPBox.IsEnabled = true;
			List<ProductUnit> productUnits = _productUnits.Where(x => x.Product == SelectedProduct).ToList();
			Unit.ItemsSource = productUnits;
			Unit.SelectedIndex = 0;
			TotalMRP = currentSale.SellType ? productUnits[0].UnitBuyPrice : productUnits[0].UnitSellPrice;
			Notify(nameof(TotalMRP));
			ChangeProductBtn.Visibility = Visibility.Visible;
		}

		private void SellType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RadioButtons radioButtons = sender as RadioButtons;
			if((String)radioButtons.SelectedItem == "Buy")
			{
				ItemSearchBox.Header = "Raw Material";
				PersonSearchBox.Header = "Vendor";
			}
			else
			{
				ItemSearchBox.Header = "Product";
				PersonSearchBox.Header = "Customer";
			}
		}

		private void PersonSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			bool isCustomer = SaleTypeSelection.SelectedIndex == 0 ? true : false;
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
			currentSale.SellType = SaleTypeSelection.SelectedIndex == 0 ? true : false;
			currentSale.CreatedDate = DateTime.Now;
			CancelSale.Visibility = Visibility.Visible;
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
			if(SaleTypeSelection.SelectedIndex == 0 && SelectedProduct != null)
			if (SelectedProduct.IsUnitsConnected)
            {
				Title = "Warning";
				decimal StockDecressed = order.ProductUnit.Conversion * order.Quantity;
				if(SelectedProduct.ProductUnits.Where(x=>x.IsBasicUnit).FirstOrDefault().Stock < StockDecressed)
                {
					MessageText = "Your are out of stock for place this order.";
					isSecoundryButtonEnabled = true;
                }
            }
			else
            {
				if(order.ProductUnit.Stock < order.Quantity)
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
			foreach (RowDefinition row in prod_raw_lbl.RowDefinitions)
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
			if (AmountBox.Visibility == Visibility.Visible || currentSale == null || currentSale.order_details.Count <= 0 )
				return;
			Button paymentButton = sender as Button;
			AmountBox.Visibility = Visibility.Visible;
			PaymentDone.Visibility = Visibility.Visible;
			CancelPayment.Visibility = Visibility.Visible;
			if (paymentButton.Name == CheckButton.Name)
			{
				CheckNumberBox.Visibility = Visibility.Visible;
				AmountBox.Value = (double)(_totalAmountToPaid - _paidViaCash);
				_paidViaCheck = 0;
			}
			else
			{
				AmountBox.Value = (double)(_totalAmountToPaid - _paidViaCheck);
				_paidViaCash = 0;
			}
		}

		private void PaymentDoneButton_Click(object sender, RoutedEventArgs e)
        {
			if (AmountBox.Value != 0.00)
            {
                Transaction transaction = new Transaction
                {
                    Amount = (decimal)AmountBox.Value,
                    CreatedDate = DateTime.Now
                };
                if (CheckNumberBox.Visibility == Visibility.Visible)
                {
                    TransactionProperty transactionProperty = new TransactionProperty
                    {
                        PropertyName = "CheckNumber",
                        PropertyValue = CheckNumberBox.Text
                    };
                    transaction.PaymentType = 2;
					transaction.TransactionProperties.Add(transactionProperty);
					_paidViaCheck += (decimal)AmountBox.Value;
                }
				else
                {
					transaction.PaymentType = 1;
					_paidViaCash += (decimal)AmountBox.Value;
				}

				currentSale.Transactions.Add(transaction);
				
			}
			AmountBox.Visibility = Visibility.Hidden;
			PaymentDone.Visibility = Visibility.Hidden;
			CheckNumberBox.Visibility = Visibility.Hidden;
			CancelPayment.Visibility = Visibility.Hidden;
			NotifyAll();
		}

        private void CancelPayment_Click(object sender, RoutedEventArgs e)
        {
			AmountBox.Visibility = Visibility.Hidden;
			PaymentDone.Visibility = Visibility.Hidden;
			CheckNumberBox.Visibility = Visibility.Hidden;
			CancelPayment.Visibility = Visibility.Hidden;
		}

        private async void FinishSale_Click(object sender, RoutedEventArgs e)
        {
			if (currentSale == null)
				return;
			string Title = "Error while saving"; ;
			string MessageText = "";
			if (_totalAmountToPaid > _paidViaCheck + _paidViaCash)
            {
				MessageText = "Please do the full Payment";
			}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
				_ = await messageBoxDialog.ShowAsync();
				return;
			}
			foreach (order_details details in currentSale.order_details)
			{
				if (details.Product.IsRawMaterial && details.Product.IsProduct)
				{
					ProductUnit productUnit;
					if (details.Product.IsUnitsConnected)
					{
						productUnit = details.Product.ProductUnits.Where(x=>x.IsBasicUnit).FirstOrDefault();
						if (currentSale.SellType == true)
							productUnit.Stock -= details.Quantity * details.ProductUnit.Conversion;
						else
							productUnit.Stock += details.Quantity * details.ProductUnit.Conversion;
					}
					else
					{
						if (currentSale.SellType == true)
							details.ProductUnit.Stock -= details.Quantity;
						else
							details.ProductUnit.Stock += details.Quantity;
					}
				}
			}
			currentSale = db.Sales.Add(currentSale);
			db.SaveChanges();
			SaleDetails billReciept = new SaleDetails(currentSale);
			Frame.Navigate(billReciept);
			NewSaleStarted();
			SaleTypeSelection.Focus();
		}

        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
			NumberBox numberBox = sender as NumberBox;
			ProductUnit measureUnit = Unit.SelectedItem as ProductUnit;
			decimal UnitPrice = SaleTypeSelection.SelectedIndex == 0 ? measureUnit.UnitSellPrice : measureUnit.UnitBuyPrice;
			TotalMRP = decimal.Round((UnitPrice * (decimal)numberBox.Value), 2, MidpointRounding.AwayFromZero);
			Notify(nameof(TotalMRP));
		}

        private void Unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			ComboBox comboBox = sender as ComboBox;
			ProductUnit measureUnit = comboBox.SelectedItem as ProductUnit;
			if (measureUnit == null)
				return;
			decimal UnitPrice = comboBox.SelectedIndex == 0 ? measureUnit.UnitSellPrice : measureUnit.UnitBuyPrice;
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
			ItemSearchBox.Text = "";
			Quantity.Value = 1;
			TotalMRP = 0;
			Unit.ItemsSource = null;
			SelectedProduct = null;
			TotalMRPBox.IsEnabled = false;
			Unit.IsEnabled = false;
			Quantity.IsEnabled = false;
			PersonSearchBox.IsEnabled = true; ;
			SaleTypeSelection.IsEnabled = true;
			ItemSearchBox.IsEnabled = false;
			PersonSearchBox.Text = "";
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
    }
}
