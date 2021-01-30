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
        private decimal _totalAmountPaid = 0;
        private decimal _totalTax = 0;
        private decimal _paidViaCash = 0;
        private decimal _paidViaCheck = 0;
        public String PersonName { get; set; }

        public String PersonAddress { get; set; }
        public String PersonCity { get; set; }
        public String PersonState { get; set; }
        public String PersonCountry { get; set; }
        public String PersonEmail { get; set; }
        public String PersonPhone { get; set; }

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
                return "Total Tax  :  " + _totalTax + " ₹";
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

        public String HsnNo
        {
            get
            {
                return GlobalMethods.HsnNo;
            }
        }

        public String PhoneNo
        {
            get
            {
                return GlobalMethods.phoneNo;
            }

        }

        public String Email
        {
            get
            {
                return GlobalMethods.Email;
            }
        }
        public List<ProductValues> productList { get; set; }
        public SaleDetails(Sale saleValue)
        {
            GlobalMethods.LoadCompanyDetails();
            List<ProductValues> products = new List<ProductValues>();
            InitializeComponent();
            this.DataContext = this;
            foreach (order_details product in saleValue.order_details)
            {
                ProductValues productValues = new ProductValues();
                productValues.Name = product.Product.Name;
                productValues.Quantity = product.Quantity;
                productValues.SGST = product.Product.Sgst;
                productValues.CGST = product.Product.Cgst;
                productValues.TotalPrice = product.TotalPrice;
                int ConversionRate = product.MeasureUnit.Conversion;
                productValues.Price = (saleValue.SellType ? product.Product.SellPrice : product.Product.BuyPrice) * ConversionRate;
                productValues.Unit = product.MeasureUnit.UnitName;
                Decimal productBasePrice = decimal.Round(productValues.Price / (1 + (product.Product.Cgst + product.Product.Sgst) / 100), 2, MidpointRounding.AwayFromZero);
                _totalAmountPaid += product.TotalPrice;
                Decimal _totalCgstTax = decimal.Round(productBasePrice * product.Product.Cgst / 100, 2, MidpointRounding.AwayFromZero) * ConversionRate * product.Quantity;
                Decimal _totalSgstTax = decimal.Round(productBasePrice * product.Product.Sgst / 100, 2, MidpointRounding.AwayFromZero) * ConversionRate * product.Quantity;
                _totalTax = _totalTax + _totalCgstTax + _totalSgstTax;
                products.Add(productValues);
            }
            Transaction transaction;
            transaction = saleValue.Transactions.Where(x => x.PaymentType == 1).FirstOrDefault();
            _paidViaCash = transaction != null ? transaction.Amount : 0;
            transaction = saleValue.Transactions.Where(x => x.PaymentType == 2).FirstOrDefault();
            _paidViaCheck = transaction != null ? transaction.Amount : 0;
            string _checkNumber = (transaction != null && transaction.PaymentType == 2) ? transaction.TransactionProperties.Where(x => x.PropertyName == "CheckNumber").FirstOrDefault().PropertyValue : "";
            CheckNumberValue = (!_checkNumber.Equals("")) ? "Check Number is" + _checkNumber : "";
            productList = products.ToList();
            PersonName = saleValue.Person.PersonName;
            PersonAddress = saleValue.Person.Address + "";
            PersonCity = saleValue.Person.City + "";
            PersonState = saleValue.Person.State + "";
            PersonCountry = saleValue.Person.Country + "";
            PersonEmail = saleValue.Person.Email + "";
            PersonPhone = saleValue.Person.Phone + "";
        }

        public class ProductValues
        {
            public String Name { get; set; }
            public Decimal Quantity { get; set; }
            public String Unit { get; set; }
            public Decimal Price { get; set; }
            public Decimal CGST { get; set; }
            public Decimal SGST { get; set; }
            public Decimal TotalPrice { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyAll()
        {
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
            Frame.GoBack();
		}
	}
}
