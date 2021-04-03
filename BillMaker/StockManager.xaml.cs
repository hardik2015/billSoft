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
using BillMaker.DataLib;
using System.ComponentModel;
using System.Data.Entity;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for StockManager.xaml
	/// </summary>
	public partial class StockManager : INotifyPropertyChanged
    {
        BillMakerEntities db = new BillMakerEntities();
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
                CurrentProductUnit = SelectedProduct.ProductUnits.FirstOrDefault();
                UnitListCombo.Visibility = Visibility.Visible;
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
            string Title = "Error while processing";
            string MessageText = "";
            if (SelectedProduct == null)
			{
                MessageText = "Select any product or raw material ";
            }
            else if(SelectedProduct.IsUnitsConnected && CurrentProductUnit == null)
			{
                MessageText = "Select product have units connected and no basic unit is set please do that?";
            }
            if(!MessageText.Equals(""))
			{
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
            var date = fromDateTime.Date;
            if (SelectedProduct.IsUnitsConnected)
			{
                var tStockAtStart = _productUnits.Where(x => x.ProductId == SelectedProduct.Id)
                    .Join(db.StockLogs.Where(x => x.AddedDate < fromDateTime), unit => unit.Id, stock => stock.ProductUnitId, (unit, stock) => unit.Conversion * stock.AddedValue);
                if (tStockAtStart.Count() == 0)
                    StockAtStart = 0;
                else
                    StockAtStart = tStockAtStart.Sum();

                stockLogs = _productUnits.Where(x => x.ProductId == SelectedProduct.Id)
                    .Join(db.StockLogs.Where(x => x.AddedDate >= fromDateTime && x.AddedDate <= toDateTime), unit => unit.Id, stock => stock.ProductUnitId, (unit, stock) => stock)
                    .OrderBy(x => x.AddedDate).ToList();
            }
            else
			{
                var tStockAtStart = db.StockLogs.Where(stock => stock.ProductUnitId == CurrentProductUnit.Id && stock.AddedDate < fromDateTime).Select(stock => stock.AddedValue);
                if (tStockAtStart.Count() == 0)
                    StockAtStart = 0;
                else
                    StockAtStart = tStockAtStart.Sum();

                stockLogs = db.StockLogs.Where(stock => stock.ProductUnitId == CurrentProductUnit.Id && stock.AddedDate >= fromDateTime && stock.AddedDate <= toDateTime)
                    .OrderBy(x => x.AddedDate).ToList();
            }
            DateTime tickingDateTime = fromDateTime.Date;
            DateTime sDate,eDate;
            DateTime demoTime = fromDateTime.AddMonths(1).AddDays(-1).Date;
            sDate = tickingDateTime;
            bool isPerMonth = DataShowSelection.SelectedIndex == 0;
			List<DataGridDataForStock> StockData = new List<DataGridDataForStock>();

			foreach (StockLog stock in stockLogs)
			{
				while (true)
				{
					if (isPerMonth && stock.AddedDate.Date >= tickingDateTime.AddMonths(1).AddDays(-1).Date)
					{
						if (AddedStock == 0 && UsedStock == 0)
						{
							tickingDateTime = tickingDateTime.AddMonths(1);
							sDate = tickingDateTime;
							continue;
						}
						else
						{
							eDate = tickingDateTime.AddMonths(1).AddDays(-1);
							StockatEnd = StockAtStart + AddedStock - UsedStock;
							DataGridDataForStock dataGridDataForStock = new DataGridDataForStock(StockAtStart, AddedStock, UsedStock, StockatEnd, sDate, eDate);
							StockData.Add(dataGridDataForStock);
							StockAtStart = StockatEnd;
							AddedStock = UsedStock = StockatEnd = 0;
							tickingDateTime = tickingDateTime.AddMonths(1);
							sDate = tickingDateTime;
						}
					}
					else if (!isPerMonth && stock.AddedDate.Date >= tickingDateTime.AddDays(1).Date)
					{
						if (AddedStock == 0 && UsedStock == 0)
						{
							tickingDateTime = tickingDateTime.AddDays(1);
							sDate = tickingDateTime;
							continue;
						}
						else
						{
							eDate = sDate;
							StockatEnd = StockAtStart + AddedStock - UsedStock;
							DataGridDataForStock dataGridDataForStock = new DataGridDataForStock(StockAtStart, AddedStock, UsedStock, StockatEnd, sDate, eDate);
							StockData.Add(dataGridDataForStock);
							StockAtStart = StockatEnd;
							AddedStock = UsedStock = StockatEnd = 0;
							tickingDateTime = tickingDateTime.AddDays(1);
							sDate = tickingDateTime;
						}
					}
					else
					{
						break;
					}
				}

				if (stock.AddedValue > 0)
					AddedStock += stock.AddedValue * stock.ProductUnit.Conversion;
				else
					UsedStock -= stock.AddedValue * stock.ProductUnit.Conversion;
			}

            eDate = toDateTime.Date;
			StockatEnd = StockAtStart + AddedStock - UsedStock;
            DataGridDataForStock dataGridDataForStockLast = new DataGridDataForStock(StockAtStart, AddedStock, UsedStock, StockatEnd,sDate,eDate);
            StockData.Add(dataGridDataForStockLast);
            StockList = StockData;
            if (!isPerMonth)
                StockLogGrid.Columns[1].Visibility = Visibility.Hidden;
            else
                StockLogGrid.Columns[1].Visibility = Visibility.Visible;
            Notify(nameof(StockList));
        }

		private void SellType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void UnitListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            ComboBox comboBox = sender as ComboBox;
            CurrentProductUnit = comboBox.SelectedItem as ProductUnit;
		}
	}

	public class DataGridDataForStock
	{
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal StockAtStart{ get; set; }
        public decimal AddedStock { get; set; }
        public decimal UsedStock { get; set; }
        public decimal StockatEnd { get; set; }

        public DataGridDataForStock(decimal start,decimal added, decimal used,decimal end,DateTime DStart, DateTime DEnd)
		{
            StartDate = DStart;
            EndDate = DEnd;
            StockatEnd = end;
            StockAtStart = start;
            UsedStock = used;
            AddedStock = added;
		}

    }
}
