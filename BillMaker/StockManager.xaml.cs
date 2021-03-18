using ModernWpf.Controls;
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
using System.Windows.Shapes;
using BillMaker.DataConnection;
using System.ComponentModel;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for StockManager.xaml
	/// </summary>
	public partial class StockManager : INotifyPropertyChanged
    {
        MyAttachedDbEntities db = new MyAttachedDbEntities();
        List<Product> _products;
        List<ProductUnit> _productUnits;

		public event PropertyChangedEventHandler PropertyChanged;

		public StockManager()
        {
            _products = db.Products.Where(x => x.IsActive).ToList();
            _productUnits = db.ProductUnits.Where(x => x.IsActive).ToList();
            InitializeComponent();
            this.DataContext = this;
            ToDate.SelectedDate = DateTime.Now;
            FromDate.SelectedDate = DateTime.Now;
        }

        public Product SelectedProduct
        {
            get; set;
        }

        public ProductUnit CurrentProductUnit
        {
            get; set;
        }

        public List<DataGridDataForStock> StockList
        {
            get;
            set;
        }

        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            ChangeProductBtn.Visibility = Visibility.Visible;
            SaleTypeSelection.IsEnabled = false;
            if (SelectedProduct.IsUnitsConnected)
			{
                CurrentProductUnit = SelectedProduct.ProductUnits.Where(t => t.IsBasicUnit).FirstOrDefault();
			}
            else
			{
                UnitListCombo.ItemsSource = SelectedProduct.ProductUnits.ToList();

			}
        }
        private void ChangeProductBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedProduct = null;
            CurrentProductUnit = null;
            ChangeProductBtn.Visibility = Visibility.Hidden;
            SaleTypeSelection.IsEnabled = true;
            ItemSearchBox.IsEnabled = true;
            ItemSearchBox.Text = "";
            UnitListCombo.Visibility = Visibility.Hidden;
        }

		private async void ShowDataBtn_Click(object sender, RoutedEventArgs e)
		{
            if(SelectedProduct == null)
			{
                string Title = "Error while processing";
                string MessageText = "Select any product or raw material ";
                MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
                _ = await messageBoxDialog.ShowAsync();
                return;
            }
            DateTime toDateTime = ToDate.SelectedDate.Value;
            DateTime fromDateTime = FromDate.SelectedDate.Value;
            decimal StockAtStart=0;
            decimal AddedStock=0;
            decimal UsedStock=0;
            decimal StockatEnd=0;
            List<StockLog> stockLogs;
            if (SelectedProduct.IsUnitsConnected)
			{
                StockAtStart = (from units in _productUnits
                                      join stock in db.StockLogs on units.Id equals stock.ProductUnitId
                                      where units.ProductId == SelectedProduct.Id && stock.AddedDate.Date < fromDateTime.Date
                                      select units.Conversion * stock.AddedValue).Sum();
                stockLogs = (from units in _productUnits
                                      join stock in db.StockLogs on units.Id equals stock.ProductUnitId
                                      where units.ProductId == SelectedProduct.Id && stock.AddedDate.Date >= fromDateTime.Date && stock.AddedDate.Date <= toDateTime.Date
                                        select stock).ToList();

            }
            else
			{
                StockAtStart = (from stock in db.StockLogs 
                                     where stock.AddedDate < fromDateTime && stock.ProductUnitId == CurrentProductUnit.Id
                                     select stock.AddedValue).Sum();
                stockLogs = (from stock in db.StockLogs
                                      where stock.AddedDate >= fromDateTime && stock.AddedDate <= toDateTime && stock.ProductUnitId == CurrentProductUnit.Id
                                      select stock).ToList();
            }
            foreach (StockLog stock in stockLogs)
            {
                if (stock.AddedValue > 0)
                    AddedStock += stock.AddedValue * stock.ProductUnit.Conversion;
                else
                    UsedStock -= stock.AddedValue * stock.ProductUnit.Conversion;
            }
            StockatEnd = StockAtStart + AddedStock - UsedStock;
            DataGridDataForStock dataGridDataForStock = new DataGridDataForStock(StockAtStart,AddedStock,UsedStock,StockatEnd);
            List<DataGridDataForStock> StockData = new List<DataGridDataForStock>();
            StockData.Add(dataGridDataForStock);
            StockList = StockData;
            Notify(nameof(StockList));
        }

		private void SellType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
	}

	public class DataGridDataForStock
	{
        public decimal StockAtStart{ get; set; }
        public decimal AddedStock { get; set; }
        public decimal UsedStock { get; set; }
        public decimal StockatEnd { get; set; }

        public DataGridDataForStock(decimal start,decimal added, decimal used,decimal end)
		{
            StockatEnd = end;
            StockAtStart = start;
            UsedStock = used;
            AddedStock = added;
		}

    }
}
