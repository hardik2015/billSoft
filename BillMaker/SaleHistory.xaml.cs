using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
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
using ModernWpf.Controls;

namespace BillMaker
{
    /// <summary>
    /// Interaction logic for SaleHistory.xaml
    /// </summary>
    public partial class SaleHistory : INotifyPropertyChanged
    {
        MyAttachedDbEntities db = new MyAttachedDbEntities();
        List<SaleTruncetDetails> _saleList;
        List<Sale> _allSaleDetails;
        List<Person> _person;

        public Person SelectedPerson
        {
            get; set;
        }

        public SaleHistory()
        {
            _person = db.People.ToList();
            _allSaleDetails = db.Sales.ToList();
            _saleList = new List<SaleTruncetDetails>();
            InitializeComponent();
            FromDate.SelectedDate = DateTime.Today;
            ToDate.SelectedDate = DateTime.Today;
            this.DataContext = this;
            List<Sale> saleDetails = _allSaleDetails.Where(x => x.CreatedDate.Date == DateTime.Today && x.SellType == true).ToList();
            foreach(Sale sale in saleDetails)
			{
                SaleTruncetDetails saleTruncet = new SaleTruncetDetails();
                saleTruncet.SaleId = sale.Id;
                saleTruncet.PersonName = sale.Person.PersonName;
                saleTruncet.CreatedDateTime = sale.CreatedDate;
                saleTruncet.TotalPrice = saleTruncet.TotalAmount = saleTruncet.TotalTax = saleTruncet.Items =  0;
                foreach(order_details order in sale.order_details)
				{
                    order.Calculate();
                    saleTruncet.TotalPrice += order.TotalTaxCalculatedPrice;
                    saleTruncet.TotalTax += order.TotalSgstPrice + order.TotalCgstPrice;
                    saleTruncet.TotalAmount += order.TotalPrice;
                    saleTruncet.Items++;
				}
                saleTruncet.TotalPrice = decimal.Round(saleTruncet.TotalPrice, 2, MidpointRounding.AwayFromZero);
                saleTruncet.TotalTax = decimal.Round(saleTruncet.TotalTax, 2, MidpointRounding.AwayFromZero);
                saleTruncet.TotalAmount = decimal.Round(saleTruncet.TotalAmount, 2, MidpointRounding.AwayFromZero);
                _saleList.Add(saleTruncet);
			}
        }

        public List<SaleTruncetDetails> saleList
        {
            get
			{
                return _saleList.ToList();
			}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isSale = SaleTypeSelection.SelectedIndex == 0 ? true : false;
            _saleList.Clear();
            DateTime fromDate = FromDate.SelectedDate.Value;
            DateTime toDate = ToDate.SelectedDate.Value;
            List<Sale> saleDetails = _allSaleDetails.Where(x => x.CreatedDate.Date <= toDate.Date && x.CreatedDate.Date >= fromDate.Date && x.SellType == isSale).ToList();
            if(SelectedPerson is Person)
            {
                saleDetails = saleDetails.Where(x => x.Person == SelectedPerson).ToList();
            }
            foreach (Sale sale in saleDetails)
            {
                SaleTruncetDetails saleTruncet = new SaleTruncetDetails();
                saleTruncet.SaleId = sale.Id;
                saleTruncet.PersonName = sale.Person.PersonName;
                saleTruncet.CreatedDateTime = sale.CreatedDate;
                saleTruncet.TotalPrice = saleTruncet.TotalAmount = saleTruncet.TotalTax = saleTruncet.Items = 0;
                foreach (order_details order in sale.order_details)
                {
                    order.Calculate();
                    saleTruncet.TotalPrice += order.TotalTaxCalculatedPrice;
                    saleTruncet.TotalTax += order.TotalSgstPrice + order.TotalCgstPrice;
                    saleTruncet.TotalAmount += order.TotalPrice;
                    saleTruncet.Items++;
                }
                saleTruncet.TotalPrice = decimal.Round(saleTruncet.TotalPrice, 2, MidpointRounding.AwayFromZero);
                saleTruncet.TotalTax = decimal.Round(saleTruncet.TotalTax, 2, MidpointRounding.AwayFromZero);
                saleTruncet.TotalAmount = decimal.Round(saleTruncet.TotalAmount, 2, MidpointRounding.AwayFromZero);
                _saleList.Add(saleTruncet);
            }
            SelectedPerson = null;
            PersonSearchBox.Text = null;
            Notify(nameof(saleList));
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            SaleTruncetDetails saleTruncate = SaleGrid.SelectedItem as SaleTruncetDetails;
            Sale viewSale = _allSaleDetails.Where(x => x.Id == saleTruncate.SaleId).FirstOrDefault();
            if(viewSale != null)
            {
                SaleDetails saleDetails = new SaleDetails(viewSale);
                saleDetails.CloseBtn.Visibility = Visibility.Visible;
                Frame.Navigate(saleDetails);
            }
        }
        private void PersonSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            bool isVendor = SaleTypeSelection.SelectedIndex == 1 ? true : false;
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Person> suggestions = GlobalMethods.searchPerson(sender.Text, "Name", _person, isVendor);

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

    }

    public class SaleTruncetDetails
	{
        public int SaleId { get; set; }
        public string PersonName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int Items { get; set; }
        public Decimal TotalPrice { get; set; }
        public Decimal TotalTax { get; set; }
        public Decimal TotalAmount { get; set; }
    }
}
