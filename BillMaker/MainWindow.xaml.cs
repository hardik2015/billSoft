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
using BillMaker.DataConnection;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MyAttachedDbEntities dbEntities = new MyAttachedDbEntities();
        public int SelectedTabIndex { get; set; }
		public  MainWindow()
		{
            log4net.Config.XmlConfigurator.Configure();
            try
            {
                log.Info("Start Processing Main Window");
                InitializeComponent();
                this.DataContext = this;
                tabData = new ControlPagesData();
                TabControlMenu.SelectedIndex = 0;
                log.Info("End Loding Mainwindow");
                
            }
            catch(Exception e)
            {
                log.Error("Exception : " + e.ToString());
            }


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
        private void LienceCheckSucess(object sender,EventArgs e)
        {
            
        }
        private void LicenceCheckFail(object sender, EventArgs e)
        {

            Close();
        }

        private async void  Window_Loaded(object sender, RoutedEventArgs e)
        {
            Sale sale = dbEntities.Sales.AsEnumerable().LastOrDefault();
            if (sale == null || sale.CreatedDate < DateTime.Now)
            {
                LicenseCheck licenseCheck = new LicenseCheck();
                await licenseCheck.VerifyProdutAsync();
                if (!licenseCheck.licenseResult[1].Equals(licenseCheck.GetRequestHash(true)))
                {
                    MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error In Licence", licenseCheck.licenseResult[2]);
                    _ = await messageBoxDialog.ShowAsync();
                    Close();
                }
            }
            ProcessGoingOn.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }
    }

    public class ControlPagesData : List<ControlInfoDataItem>
    {
        public ControlPagesData()
        {
            AddPage(typeof(PointOfSale),"Point Of Sale");
            AddPage(typeof(ProductPage), "Products/Raw Material");
            AddPage(typeof(PersonPage), "Customer/ Vendor");
            AddPage(typeof(CompanySettingPage), "Company Settings");
            AddPage(typeof(SaleHistory), "Sale History");
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
