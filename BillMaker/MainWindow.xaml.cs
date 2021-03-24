﻿using System;
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
                tabData = new ControlPagesData();
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
				/*LicenseCheck licenseCheck = new LicenseCheck();
                if( !licenseCheck.VerifyProdutLocal())
				{
                    DateTime expiryDateTime;
                    await licenseCheck.VerifyProdutAsync();
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime.TryParseExact(licenseCheck.licenseResult[0], "dd/MM/yyyy", provider, DateTimeStyles.AssumeLocal, out expiryDateTime);
                    string ErrorString = "";
                    if (!licenseCheck.licenseResult[1].Equals(licenseCheck.GetRequestHash(true)))
                    {
                        ErrorString = licenseCheck.licenseResult[2];
                    }
                    else if (expiryDateTime < DateTime.UtcNow)
                    {
                        ErrorString = "Your Licence Expired \n please Renew it";
                    }
                    if (!ErrorString.Equals(""))
                    {
                        MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error In Licence", ErrorString);
                        _ = await messageBoxDialog.ShowAsync();
                        Close();
                    }
                }
                licenseCheck.UpdateLocalData();*/
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
        public ControlPagesData()
        {
            AddPage(typeof(PointOfSale),"Point Of Sale");
            AddPage(typeof(ProductPage), "Products/Raw Material");
            AddPage(typeof(PersonPage), "Customer/ Vendor");
            AddPage(typeof(CompanySettingPage), "Company Settings");
            AddPage(typeof(SaleHistory), "Sale History");
            AddPage(typeof(UnitConfig), "Unit Configuration");
            AddPage(typeof(StockManager), "Stock Management");
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
