using System;
using System.Collections.Generic;
using ModernWpf.Controls;
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
using BillMaker.DataLib;
using System.ComponentModel;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for Vouchers.xaml
	/// </summary>
	public partial class VouchersPage : INotifyPropertyChanged
	{
		BillMakerEntities db = new BillMakerEntities();
		List<Product> _products;
        List<Person> _people;
		List<ProductUnit> _productUnits;
        List<Voucher> _selectedProductVouchers;

		public VouchersPage()
		{
            _people = db.People.Where(x => x.IsActive && x.PersonId != 1).ToList();
			_products = db.Products.Where(x => x.IsActive).ToList();
			_productUnits = db.ProductUnits.Where(x => x.IsActive).ToList();
			InitializeComponent();
			this.DataContext = this;
			SelectedProductUnit = new ProductUnit();
		}

		public Product SelectedProduct
		{
			get; set;
		}

        public ProductUnit SelectedProductUnit
        {
            get; set;
        }

        public Person SelectedPerson { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

        public List<Voucher> ProductVouchersList
        {
            get
            {
                if (SelectedProduct == null)
                    return new List<Voucher>();
                else
                    return _selectedProductVouchers.ToList();
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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

        private void Search_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            SelectedProduct = args.SelectedItem as Product;
            ItemSearchBox.IsEnabled = false;
            VoucherGrid.Visibility = Visibility.Visible;
            ChangeProductBtn.Visibility = Visibility.Visible;
            List<ProductUnit> productUnits = _productUnits.Where(x => x.ProductId == SelectedProduct.Id).ToList();
            SelectedProductUnit = productUnits.FirstOrDefault();
            VoucherProductUnit.ItemsSource = productUnits;
            VoucherProductUnit.SelectedIndex = 0;
            VoucherAddDate.SelectedDate = DateTime.Now;
            _selectedProductVouchers = db.Vouchers.Where(voucher=>voucher.ProductId == SelectedProduct.Id).ToList();
            SaleTypeSelection.IsEnabled = false;
            NotifyAll();

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
        }


        private void PersonSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyAll()
        {
            Notify(nameof(ProductVouchersList));
        }

        private void StockProductUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            ProductUnit productUnit = (sender as ComboBox).SelectedItem as ProductUnit;
            SelectedProductUnit = productUnit;
		}

		private async void AddVoucherBtn_Click(object sender, RoutedEventArgs e)
		{
            String ErrorMsg = "";
            if(SelectedProductUnit == null || SelectedPerson == null)
			{
                ErrorMsg = "Please set the product unit  or add product unit in unit configuration screen";
               
            }
            else if( SelectedPerson == null)
			{
                ErrorMsg = "Please select client";
			}
            else if(VoucherValueBox.Value == 0)
			{
                ErrorMsg = "Please add voucher quntity value";
			}
            if(ErrorMsg.Length != 0)
			{
                MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error !!", ErrorMsg);
                _ = await messageBoxDialog.ShowAsync();
                return;
            }
            DateTime voucherDate = VoucherAddDate.SelectedDate.Value.Date;
            Voucher voucher = new Voucher();
            voucher.AddedDate = voucherDate;
            voucher.PersonId = SelectedPerson.PersonId;
            voucher.VoucherType = SaleTypeSelection.SelectedIndex == 0;
            voucher.Product = SelectedProduct;
            voucher.ProductUnit = SelectedProductUnit;
            voucher.ValueAdded = Decimal.Round((Decimal)VoucherValueBox.Value, 2, MidpointRounding.AwayFromZero);
            StockLog stock = new StockLog();
            stock.AddedDate = voucherDate;
            stock.AddedValue = -voucher.ValueAdded;
            stock.ProductUnit = SelectedProductUnit;
            if (SelectedProductUnit.Product.IsUnitsConnected)
            {
                ProductUnit pU = SelectedProductUnit.Product.ProductUnits.Where(t => t.IsBasicUnit).FirstOrDefault();
                if (pU == null)
                {
                    MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error !!", "Please set the basic unit for this product");
                    _ = await messageBoxDialog.ShowAsync();
                    return;
                }
                pU.Stock -= stock.AddedValue * SelectedProductUnit.Conversion;
                db.ProductUnits.Where(x => x.Id == pU.Id).FirstOrDefault().Stock = pU.Stock;
            }
            else
            {
                SelectedProductUnit.Stock -= stock.AddedValue;
                db.ProductUnits.Where(x => x.Id == SelectedProductUnit.Id).FirstOrDefault().Stock = SelectedProductUnit.Stock;
            }
            voucher = db.Vouchers.Add(voucher);
            db.StockLogs.Add(stock);
            db.SaveChanges();
            _selectedProductVouchers.Add(voucher);
            NotifyAll();
        }

		private void SellType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            RadioButtons radioButtons = sender as RadioButtons;
            if (radioButtons.SelectedIndex == 1)
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

		private void ChangeProductBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnEdit_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
