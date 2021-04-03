using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using log4net;
using BillMaker.DataLib;
using System.Globalization;
using System.Net.Http;
using System.Net;
using BillMaker.LicenseArgs;
using Newtonsoft.Json.Linq;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        BillMakerEntities dbEntities = new BillMakerEntities();
        public int SelectedTabIndex { get; set; }
		public  MainWindow()
		{
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            log4net.Config.XmlConfigurator.Configure();
            try
            {
                log.Info("Start Loading Main Window ");
                InitializeComponent();
                this.DataContext = this;
                bool isVoucherEnabled = dbEntities.CompanySettings.Where(setting => setting.Name.Equals("VoucherEnabled")).FirstOrDefault().Value.Equals("1");
                tabData = new ControlPagesData(isVoucherEnabled);
                TabControlMenu.SelectedIndex = 0;
                log.Info("End Loading Main window");
                
            }
            catch(Exception e)
            {
                log.Error("Exception In Main Window : " + e.ToString());
            }
            GlobalMethods.MainFrameMargin = scrollViewer.Margin.Top;
        }

        public List<ControlInfoDataItem> tabData
		{
			get;
			set;
		}
		private void TabControlMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TabControl tab = sender as TabControl;
			Type tabItem = (tab.SelectedItem as ControlInfoDataItem).PageType;
			MainFrame.Navigate(tabItem);
		}

        private void CloseAppBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
			{
                LicenseCheckingResponse licenseCheckingResponse = null;
                LicenseCheck licenseCheck = new LicenseCheck();
                String expiryDate = "";
                if (!licenseCheck.VerifyProdutLocal())
                {
                    HttpResponseMessage httpResponse = await licenseCheck.VerifyProdutAsync();
                    string ErrorString = "";
                    if (httpResponse.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        licenseCheckingResponse = await httpResponse.Content.ReadAsAsync<LicenseCheckingResponse>();
                        DateTime expiryDateTime;
                        expiryDate = Encoding.UTF8.GetString(Convert.FromBase64String(licenseCheckingResponse.ExpDate));
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        DateTime.TryParseExact(expiryDate, "dd-MM-yyyy", provider, DateTimeStyles.AssumeLocal, out expiryDateTime);
                        if(expiryDateTime < DateTime.Now )
						{
                            ErrorString = "Your Licence Expired \n please Renew it";
                        }
                    }
                    else
                    {
                        String jSon = await httpResponse.Content.ReadAsStringAsync();
                        var JsonArray = JObject.Parse(jSon);
                        ErrorString = Convert.ToString(JsonArray["Message"]);
                    }
                    if (!ErrorString.Equals(""))
                    {
                        MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error In Licence", ErrorString);
                        _ = await messageBoxDialog.ShowAsync();
                        Close();
                    }
                }
                if(licenseCheckingResponse == null)
				{
                    licenseCheck.UpdateLocalData("");
				}
                else
				{
                    licenseCheck.UpdateLocalData(expiryDate);
				}
			}
            catch(Exception exc)
            {
                log.Error("Exception In Main Window : " + exc.ToString());
            }
            ProcessGoingOn.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }

    public class ControlPagesData : List<ControlInfoDataItem>
    {
        public ControlPagesData(bool isVoucherEnabled)
        {
            AddPage(typeof(PointOfSale),"Point Of Sale");
            AddPage(typeof(ProductPage), "Products/Raw Material");
            AddPage(typeof(PersonPage), "Customer/ Vendor");
            AddPage(typeof(CompanySettingPage), "Company Settings");
            AddPage(typeof(SaleHistory), "Sale History");
            AddPage(typeof(UnitConfig), "Unit Configuration");
            AddPage(typeof(StockManager), "Stock Management");
            AddPage(typeof(AccountSattlement), "Account Sattlement");
            AddPage(typeof(AccountLog), "Account Statustics");
            if(isVoucherEnabled)
                AddPage(typeof(VouchersPage), "Vouchers");
        }

        private void AddPage(Type pageType, string displayName = null)
        {
            Add(new ControlInfoDataItem(pageType, displayName));
        }

    }

    public class ControlInfoDataItem
    {
        public ControlInfoDataItem(Type pageType, string title = null)
        {
            PageType = pageType;
            Title = title ?? pageType.Name.Replace("Page", null);
        }

        public string Title { get; }

        public Type PageType { get; }

        public override string ToString()
        {
            return Title;
        }
    }
}
