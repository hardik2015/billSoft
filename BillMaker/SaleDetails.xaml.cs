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
        private decimal _totalTax = 0;
        private decimal _paidViaCash = 0;
        private decimal _paidViaCheck = 0;

        public Sale sale { get; set; }

        public String TotalAmountValue
        {
            get
            {
                return "Total Amount  :  " + _totalAmountPaid + " ₹";
            }
        }

        public String TotalTaxValue
        {
            get
            {
                return "Total Tax  :  " + decimal.Round(_totalTax,2,MidpointRounding.AwayFromZero) + " ₹";
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

        public String BankRTGSNumberValue
        {
            get
            {
                return "RTGS No : " + GlobalMethods.BankRTGSNumber;
            }

        }

        public bool IsShowBankDetails
        {
            get
            {
                return GlobalMethods.IsBankDetailsVisible;
            }
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
                product.Calculate();
                _totalAmountPaid += product.TotalPrice;
                _totalTax = _totalTax + product.TotalCgstPrice + product.TotalSgstPrice;
                _totalTaxableAmount += product.TotalTaxCalculatedPrice;
            }
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
            Notify(nameof(TotalTaxValue));
            Notify(nameof(PaidViaCashValue));
            Notify(nameof(PaidViaCheckValue));
            Notify(nameof(CheckNumberValue));
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void PrintMethod()
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(Print, "Invoice");
            }
        }

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
            Frame.GoBack();
		}

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintMethod();
        }
    }
}
