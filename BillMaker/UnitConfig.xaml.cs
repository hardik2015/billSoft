using BillMaker.DataConnection;
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
    /// Interaction logic for UnitConfig.xaml
    /// </summary>
    public partial class UnitConfig : INotifyPropertyChanged
    {
        MyAttachedDbEntities db = new MyAttachedDbEntities();
        List<Product> _products;
        List<ProductUnit> _productUnits;
        List<ProductUnit> _selectedProductsUnit;

        public UnitConfig()
        {
            _products = db.Products.Where(x => x.IsActive).ToList();
            _productUnits = db.ProductUnits.Where(x => x.IsActive).ToList();
            InitializeComponent();
            this.DataContext = this;
            CurrentProductUnit = new ProductUnit();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Product SelectedProduct
        {
            get; set;
        }

        public ProductUnit CurrentProductUnit
        {
            get; set;
        }

        public ProductUnit AddStockProductUnit
		{
            get;set;
		}

        public Decimal UnitBuyValue
        {
            get
            {
                return CurrentProductUnit.UnitBuyPrice;
            }
            set
            {
                CurrentProductUnit.UnitBuyPrice = value;
            }
        }

        public Decimal UnitSellValue
        {
            get
            {
                return CurrentProductUnit.UnitSellPrice;
            }
            set
            {
                CurrentProductUnit.UnitSellPrice = value;
            }
        }

        public int ConversionValue
        {
            get
            {
                if (CurrentProductUnit.Conversion == 0)
                    CurrentProductUnit.Conversion = 1;
                return CurrentProductUnit.Conversion;
            }
            set
            {
                CurrentProductUnit.Conversion = value;
            }
        }


        public String UnitNameValue
        {
            get
            {
                return CurrentProductUnit.UnitName;
            }
            set
            {
                CurrentProductUnit.UnitName = value;
            }
        }

        public bool IsPurchaseUnitValue
        {
            get
            {
                return CurrentProductUnit.IsPurchaseUnit;
            }
            set
            {
                CurrentProductUnit.IsPurchaseUnit = value;
                Notify(nameof(IsPurchaseUnitValue));
            }
        }

        public bool IsBasicUnitValue
        {
            get
            {
                return CurrentProductUnit.IsBasicUnit;
            }
            set
            {
                CurrentProductUnit.IsBasicUnit = value;
                Notify(nameof(IsBasicUnitValue));
            }
        }

        public List<ProductUnit> ProductUnitList
        {
            get
            {
                if (SelectedProduct == null)
                    return new List<ProductUnit>();
                else
                    return _selectedProductsUnit.ToList();
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
            NewUnitGrid.Visibility = Visibility.Visible;
            ChangeProductBtn.Visibility = Visibility.Visible;
            CurrentProductUnit = new ProductUnit();
            CurrentProductUnit.Product = SelectedProduct;
            _selectedProductsUnit = _productUnits.Where(x => x.ProductId == SelectedProduct.Id).ToList();
            SaleTypeSelection.IsEnabled = false;
            ConfigureNewUnitFields();
        }
        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyAll()
        {
            Notify(nameof(UnitBuyValue));
            Notify(nameof(UnitSellValue));
            Notify(nameof(UnitNameValue));
            Notify(nameof(IsBasicUnitValue));
            Notify(nameof(IsPurchaseUnitValue));
            Notify(nameof(ConversionValue));
            Notify(nameof(ProductUnitList));
        }
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveUnitBtn.Content.ToString() == "Edit Unit")
            {
                updateProduct();
                SaveUnitBtn.Content = "Add Unit";
            }
            else
            {
                CurrentProductUnit.Stock = 0;
                CurrentProductUnit.IsActive = true;
                CurrentProductUnit = db.ProductUnits.Add(CurrentProductUnit);
                db.SaveChanges();
                _productUnits.Add(CurrentProductUnit);
                _selectedProductsUnit.Add(CurrentProductUnit);
            }
            CurrentProductUnit = new ProductUnit();
            CurrentProductUnit.Product = SelectedProduct;
            ConfigureNewUnitFields();

        }

        private void updateProduct()
        {
            ProductUnit updateProductUnit = db.ProductUnits.Where(x => x.Id == CurrentProductUnit.Id).FirstOrDefault();
            updateProductUnit.UnitName = CurrentProductUnit.UnitName;
            updateProductUnit.UnitBuyPrice = CurrentProductUnit.UnitBuyPrice;
            updateProductUnit.UnitSellPrice = CurrentProductUnit.UnitSellPrice;
            updateProductUnit.Conversion = CurrentProductUnit.Conversion;
            updateProductUnit.IsBasicUnit = CurrentProductUnit.IsBasicUnit;
            updateProductUnit.IsPurchaseUnit = CurrentProductUnit.IsPurchaseUnit;
            db.SaveChanges();
        }

        private void SellType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RadioButtons radioButtons = sender as RadioButtons;
            if ((String)radioButtons.SelectedItem == "Buy")
            {
                ItemSearchBox.Header = "Raw Material";
            }
            else
            {
                ItemSearchBox.Header = "Product";
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ProductUnit unitRemove = unitDataGrid.SelectedItem as ProductUnit;
            if (db.order_details.Where(x => x.ProductId == unitRemove.Id).FirstOrDefault() != null)
            {
                db.ProductUnits.Where(x => x.Id == unitRemove.Id).FirstOrDefault().IsActive = false;
            }
            else
            {
                db.ProductUnits.Remove(unitRemove);
            }
            db.SaveChanges();
            _selectedProductsUnit.Remove(unitRemove);
            Notify(nameof(ProductUnitList));
            ConfigureNewUnitFields();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            SaveUnitBtn.Content = "Edit Unit"; ;
            CurrentProductUnit = unitDataGrid.SelectedItem as ProductUnit;
            ConfigureNewUnitFields();
            NotifyAll();
        }

        private void ConfigureNewUnitFields()
        {

            if (!SelectedProduct.IsUnitsConnected)
            {
                CurrentProductUnit.IsPurchaseUnit = false;
                CurrentProductUnit.Conversion = 1;
                CurrentProductUnit.IsBasicUnit = false;
                IsPurchaseUnitCheck.Visibility = Visibility.Hidden;
                ConversionRateBox.Visibility = Visibility.Hidden;
                IsBasicUnitCheck.Visibility = Visibility.Hidden;
            }
            else if (SelectedProduct.IsUnitsConnected && _selectedProductsUnit.Count == 0)
            {
                IsBasicUnitCheck.IsEnabled = false;
                CurrentProductUnit.IsBasicUnit = true;
                IsBasicUnitCheck.Visibility = Visibility.Visible;
            }
            else if (SelectedProduct.IsUnitsConnected)
            {
                ProductUnit basicUnit = _selectedProductsUnit.Where(x => x.IsBasicUnit).ToList().FirstOrDefault();
                ProductUnit purchaseUnit = _selectedProductsUnit.Where(x => x.IsPurchaseUnit).ToList().FirstOrDefault();
                if (basicUnit != null)
                {
                    BasicUnitLbl.Content = "Basic Unit : " + basicUnit.UnitName;
                }
                else
                {
                    BasicUnitLbl.Content = "Basic Unit is not Set";
                }
                if (purchaseUnit != null)
                {
                    PurchaseUnitLbl.Content = "Purchase Unit : " + purchaseUnit.UnitName;
                }
                else
                {
                    PurchaseUnitLbl.Content = "Purchase Unit is not Set";
                }
                IsBasicUnitCheck.IsEnabled = true;
                if (SelectedProduct.IsUnitsConnected && basicUnit != null && basicUnit != CurrentProductUnit)
                {
                    IsBasicUnitCheck.Visibility = Visibility.Hidden;
                }
                else if (CurrentProductUnit.IsBasicUnit)
                {
                    IsBasicUnitCheck.Visibility = Visibility.Visible;
                }
                if (SelectedProduct.IsUnitsConnected && purchaseUnit != null && purchaseUnit != CurrentProductUnit)
                {
                    IsPurchaseUnitCheck.Visibility = Visibility.Hidden;
                }
                else if (CurrentProductUnit.IsPurchaseUnit)
                {
                    IsPurchaseUnitCheck.Visibility = Visibility.Visible;
                }
            }
            NotifyAll();
        }

        private void ChangeProductBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedProduct = null;
            NewUnitGrid.Visibility = Visibility.Hidden;
            _selectedProductsUnit.Clear();
            CurrentProductUnit = new ProductUnit();
            ChangeProductBtn.Visibility = Visibility.Hidden;
            SaleTypeSelection.IsEnabled = true;
            ItemSearchBox.IsEnabled = true;
            ItemSearchBox.Text = "";
            IsPurchaseUnitCheck.Visibility = Visibility.Hidden;
            IsBasicUnitCheck.Visibility = Visibility.Hidden;
            BasicUnitLbl.Content = "";
            PurchaseUnitLbl.Content = "";
            NotifyAll();
        }

        private void UnitConfig_Loaded(object sender, RoutedEventArgs e)
        {
            unitDataGrid.Height = SystemParameters.MaximizedPrimaryScreenHeight - GlobalMethods.MainFrameMargin - UnitListGrid.Margin.Top - unitDataGrid.Margin.Top - mainGrid.Margin.Bottom;
        }

		private void btnAddStock_Click(object sender, RoutedEventArgs e)
		{
            AddStockProductUnit = unitDataGrid.SelectedItem as ProductUnit;
            StockGrid.Visibility = Visibility.Visible;
            StockValueBox.Value = 0;
            StockAddDate.SelectedDate = DateTime.Now;
		}

		private void AddStockBtn_Click(object sender, RoutedEventArgs e)
		{
            StockLog stockLog = new StockLog();
            stockLog.AddedDate = StockAddDate.SelectedDate.Value;
            stockLog.AddedValue = Decimal.Round((decimal)StockValueBox.Value, 2, MidpointRounding.AwayFromZero);
            stockLog.ProductUnit = AddStockProductUnit;
            if(AddStockProductUnit.Product.IsUnitsConnected)
			{
                ProductUnit pU = AddStockProductUnit.Product.ProductUnits.Where(t => t.IsBasicUnit).FirstOrDefault();
                pU.Stock += stockLog.AddedValue * AddStockProductUnit.Conversion;
                db.ProductUnits.Where(x=>x.Id == pU.Id).FirstOrDefault().Stock = pU.Stock;
                _selectedProductsUnit.Where(x => x.Id == pU.Id).FirstOrDefault().Stock = pU.Stock;
            }
            else
			{
                AddStockProductUnit.Stock += stockLog.AddedValue;
                db.ProductUnits.Where(x => x.Id == AddStockProductUnit.Id).FirstOrDefault().Stock = AddStockProductUnit.Stock;
                _selectedProductsUnit.Where(x => x.Id == AddStockProductUnit.Id).FirstOrDefault().Stock = AddStockProductUnit.Stock;

            }
            db.StockLogs.Add(stockLog);
            Notify(nameof(ProductUnitList));
            db.SaveChanges();
            StockGrid.Visibility = Visibility.Hidden;
		}
	}
}
