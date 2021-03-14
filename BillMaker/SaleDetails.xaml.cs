using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Printing;
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

namespace BillMaker
{
    /// <summary>
    /// Interaction logic for BillReciept.xaml
    /// </summary>
    public partial class SaleDetails : INotifyPropertyChanged
    {
        private decimal _totalTaxableAmount = 0;
        private decimal _totalAmountPaid = 0;
        private decimal _totalCgstTax = 0;
        private decimal _totalSgstTax = 0;
        private decimal _paidViaCash = 0;
        private decimal _paidViaCheck = 0;
        private double _paddingForLastRow = 0;
        private bool _anyUnitConnectedProduct = false;
        public Sale sale { get; set; }

        public String TotalAmountValue
        {
            get
            {
                return "Total Amount  :  " + _totalAmountPaid + " ₹";
            }
        }

        public String TotalCgstTaxValue
        {
            get
            {
                return "SGST Tax  :  " + decimal.Round(_totalCgstTax,2,MidpointRounding.AwayFromZero) + " ₹";
            }
        }

        public String TotalSgstTaxValue
        {
            get
            {
                return "CGST Tax  :  " + decimal.Round(_totalSgstTax, 2, MidpointRounding.AwayFromZero) + " ₹";
            }
        }

        public String TotalTaxableAmountValue
        {
            get
            {
                return "Total Taxable Amount  :  " + decimal.Round(_totalTaxableAmount, 2, MidpointRounding.AwayFromZero) + " ₹";
            }
        }
        public String PaidViaCashValue
        {
            get
            {
                if (_paidViaCash == 0)
                {
                    return "";
                }
                return "Total Paid Via Cash  :  " + _paidViaCash + " ₹";
            }
        }

        public String PaidViaCheckValue
        {
            get
            {
                if (_paidViaCheck == 0)
                {
                    return "";
                }
                return "Total Paid Via Check  :  " + _paidViaCheck + " ₹";
            }
        }

        public String CheckNumberValue
        {
            get;
            set;
        }

        public String CompanyName
        {
            get
            {
                return GlobalMethods.companyName;
            }
        }

        public String GSTINNo
        {
            get
            {
                return "GSTIN No : " + GlobalMethods.GstINNo;
            }
        }
        public String TANNo
        {
            get
            {
                return "TAN No : " + GlobalMethods.TANNo;
            }
        }

        public String PhoneNo
        {
            get
            {
                return "Phone No : " + GlobalMethods.phoneNo;
            }

        }

        public String Email
        {
            get
            {
                return "Email Id : " + GlobalMethods.Email;
            }
        }

        public String RecieptDate
        {
            get;set;
        }

        public String BankAccountNumberValue
        {
            get
            {
                return "Account Number No : " + GlobalMethods.BankAccountNumber;
            }
        }
        public String BankIFSCCodeValue
        {
            get
            {
                return "IFSC Code : " + GlobalMethods.BankIFSCCode;
            }
        }

        public bool IsShowBankDetails
        {
            get
            {
                return GlobalMethods.IsBankDetailsVisible;
            }
        }

        public List<order_details> orderDetails
        {
            get;
            set;
        }
        public SaleDetails(Sale saleValue)
        {
            sale = saleValue;
            GlobalMethods.LoadCompanyDetails();
            InitializeComponent();
            this.DataContext = this;
            if(saleValue.SellType)
            {
                PersonInfoIdentifer.Content = "Customer Info";
            }
            else
            {
                PersonInfoIdentifer.Content = "Vendor Info";
            }
            RecieptDate = saleValue.CreatedDate.ToShortDateString();
            foreach (order_details product in saleValue.order_details)
            {
                if (product.Product.IsUnitsConnected)
                    _anyUnitConnectedProduct = true;
                product.Calculate();
                _totalAmountPaid += product.TotalPrice;
                _totalCgstTax += product.TotalCgstPrice;
                _totalSgstTax += product.TotalSgstPrice;
                _totalTaxableAmount += product.TotalTaxCalculatedPrice;
            }
            List<order_details> orders = saleValue.order_details.ToList();

            orderDetails = orders;
            Transaction transaction;
            transaction = saleValue.Transactions.Where(x => x.PaymentType == 1).FirstOrDefault();
            _paidViaCash = transaction != null ? transaction.Amount : 0;
            transaction = saleValue.Transactions.Where(x => x.PaymentType == 2).FirstOrDefault();
            _paidViaCheck = transaction != null ? transaction.Amount : 0;
            string _checkNumber = (transaction != null && transaction.PaymentType == 2) ? transaction.TransactionProperties.Where(x => x.PropertyName == "CheckNumber").FirstOrDefault().PropertyValue : "";
            CheckNumberValue = (!_checkNumber.Equals("")) ? "Check Number : " + _checkNumber : "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyAll()
        {
            Notify(nameof(TotalTaxableAmountValue));
            Notify(nameof(TotalAmountValue));
            Notify(nameof(TotalCgstTaxValue));
            Notify(nameof(TotalSgstTaxValue));
            Notify(nameof(PaidViaCashValue));
            Notify(nameof(PaidViaCheckValue));
            Notify(nameof(CheckNumberValue));
            Notify(nameof(orderDetails));
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void PrintMethod()
        {
            Scroll.ScrollToTop();
            CloseBtn.Visibility = Visibility.Hidden;
            PrintBtn.Visibility = Visibility.Hidden;
            PrintDialog printDialog = new PrintDialog();

            System.Printing.PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);



            //get scale of the print wrt to screen of WPF visual

            double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / this.ActualWidth, capabilities.PageImageableArea.ExtentHeight /

                           this.ActualHeight);



            //Transform the Visual to scale

            this.LayoutTransform = new ScaleTransform(scale, scale);



            //get the size of the printer page

            Size sz = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);



            //update the layout of the visual to the printer page size.

            this.Measure(sz);

            this.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
            if(GlobalMethods.settingDefaultPrinter.Equals(""))
            {
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(Print, "Invoice");
                }
            }
            else
            {
                printDialog.PrintQueue = new PrintQueue(new PrintServer(), GlobalMethods.settingDefaultPrinter);
                try
                {
                    printDialog.PrintVisual(Print, "Invoice");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            CloseBtn.Visibility = Visibility.Visible;
            PrintBtn.Visibility = Visibility.Visible;
            
        }

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
            Frame.GoBack();
		}

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintMethod();
        }

        private void ItemList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid.Items.Count - 1 == e.Row.GetIndex() && _paddingForLastRow > 0)
            {
                e.Row.Height = _paddingForLastRow;//new Thickness(e.Row.Padding.Left, e.Row.Padding.Top, e.Row.Padding.Right,_paddingForLastRow);
            }
        }

        private void ItemList_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
        }

        private void ItemList_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (!_anyUnitConnectedProduct)
                dataGrid.Columns[2].Visibility = Visibility.Hidden;
            if(dataGrid.ActualHeight < 400)
            {
                _paddingForLastRow = 400 - dataGrid.ActualHeight;
                orderDetails = sale.order_details.ToList();
                Notify(nameof(orderDetails));
            }
        }

        private void SaleDetails_Loaded(object sender, RoutedEventArgs e)
        {
            Page page = sender as Page;
            PAddressValue.Width = page.DesiredSize.Width / 2 - 150;
        }
    }
}
