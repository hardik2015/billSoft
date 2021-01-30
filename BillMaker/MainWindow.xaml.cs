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

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public int SelectedTabIndex { get; set; }
		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;
			tabData = new ControlPagesData();
            TabControlMenu.SelectedIndex = 0;

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
