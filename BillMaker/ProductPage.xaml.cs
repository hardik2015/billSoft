using BillMaker.DataConnection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for ProductPage.xaml
	/// </summary>
	public partial class ProductPage : INotifyPropertyChanged
	{
		
		private Product currentProduct;
		private List<Product> _products;
		MyAttachedDbEntities db = new MyAttachedDbEntities();
		public event PropertyChangedEventHandler PropertyChanged;
		public Dictionary<String, String> productRawMaterialSelection { get; set; }
		public ProductPage()
		{
			InitializeComponent();
			currentProduct = new Product();
			this.DataContext = this;
			_products = db.Products.Where(x=>x.IsActive).ToList();
			currentProduct.IsProduct = false;
			currentProduct.IsRawMaterial = false;
			productRawMaterialSelection = new Dictionary<string, string>();
			productRawMaterialSelection.Add("ProductList", "Show Product List");
			productRawMaterialSelection.Add("RawMaterialList", "Show RawMaterial List");
		}
		
		public Decimal cgstValue
		{
			get
			{
				return currentProduct.Cgst;
			}
			set
			{
				currentProduct.Cgst = value;
			}
		}

		public Decimal sgstValue
		{
			get
			{
				return currentProduct.Sgst;
			}
			set
			{
				currentProduct.Sgst = value;
			}
		}

		public String descriptionValue
		{
			get
			{
				return currentProduct.description;
			}
			set
			{
				currentProduct.description = value;
			}
		}

		public String nameValue
		{
			get
			{
				return currentProduct.Name;
			}
			set
			{
				currentProduct.Name = value;
			}
		}
		public String hsnCodeValue
		{
			get
			{
				return currentProduct.HSNCode;
			}
			set
			{
				if (value.Length > 8)
					currentProduct.HSNCode = "00000000";

				currentProduct.HSNCode = value;
			}
		}

		public bool IsProductValue
		{
			get
			{
				return currentProduct.IsProduct;
			}
			set
			{
				currentProduct.IsProduct = value ;
				Notify(nameof(IsProductValue));
			}
		}

		public bool IsRawMaterialValue
		{
			get
			{
				return currentProduct.IsRawMaterial;
			}
			set
			{
				currentProduct.IsRawMaterial = value;
				Notify(nameof(IsRawMaterialValue));
			}
		}
		public List<Product> gridList
		{
			get;
			set;
		}

		public bool IsUnitConnectedValue
		{
			get
			{
				return currentProduct.IsUnitsConnected;
			}
			set
			{
				currentProduct.IsUnitsConnected = value;
				Notify(nameof(IsUnitConnectedValue));
			}
		}



		private async void Add_Click(object sender, RoutedEventArgs e)
		{
			string Title = "Error while saving"; ;
			string MessageText = "";
			if (!IsProductCheck.IsChecked.Value && !IsRawMaterialCheck.IsChecked.Value)
			{
				MessageText = "Select any one from:- \n1)Product \n2)Raw Material ";
			}
			if (!MessageText.Equals(""))
			{
				MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
				_ = await messageBoxDialog.ShowAsync();
				return;
			}

			if (SaveForm.Content.ToString() == "Edit")
			{
				updateProduct();
			}
			else
			{
				if (_products.Exists(x => x.Name == currentProduct.Name))
				{
					MessageText = "Please select diffrent name beacuse there an product with same name exists";
				}
				if (!MessageText.Equals(""))
				{
					MessageBoxDialog messageBoxDialog = new MessageBoxDialog(Title, MessageText);
					_ = await messageBoxDialog.ShowAsync();
					return;
				}
				currentProduct.IsActive = true;
				currentProduct = db.Products.Add(currentProduct);
				db.SaveChanges();
				_products.Add(currentProduct);
			}
			if (productRawMaterialCombo.SelectedIndex == 0)
			{
				gridList = _products.Where(x => x.IsProduct).OrderBy(x=>x.Name).ToList();
			}
			else
			{
				gridList = _products.Where(x => x.IsRawMaterial).OrderBy(x => x.Name).ToList();
			}
			SearchBox.Text = "";
			gridColumns.SelectedIndex = 0;
			currentProduct = new Product();
			NotifyAll();
			SaveForm.Content = "Add";
		}

		private void updateProduct()
		{
			Product updateProduct = db.Products.Where(x => x.Id == currentProduct.Id).FirstOrDefault();
			updateProduct.Name = currentProduct.Name;
			updateProduct.Cgst = currentProduct.Cgst;
			updateProduct.Sgst = currentProduct.Sgst;
			updateProduct.description = currentProduct.description;
			updateProduct.IsProduct = currentProduct.IsProduct;
			updateProduct.IsRawMaterial = currentProduct.IsRawMaterial;
			updateProduct.IsUnitsConnected = currentProduct.IsUnitsConnected;
			db.SaveChanges();
		}

		public void Notify(string propertyName)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyAll()
		{
			Notify(nameof(nameValue));
			Notify(nameof(cgstValue));
			Notify(nameof(sgstValue));
			Notify(nameof(descriptionValue));
			Notify(nameof(IsRawMaterialValue));
			Notify(nameof(IsProductValue));
			Notify(nameof(IsUnitConnectedValue));
			Notify(nameof(hsnCodeValue));
			Notify(nameof(gridList));
		}

		public void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			Button delete = sender as Button;
			Product productRemove = productGrid.SelectedItem as Product;
			if (db.order_details.Where(x => x.Product.Id == productRemove.Id).FirstOrDefault() != null)
			{
				Product product = db.Products.Where(x => x.Id == productRemove.Id).FirstOrDefault();
				product.IsActive = false;
				foreach(ProductUnit productUnit in product.ProductUnits)
                {
					productUnit.IsActive = false;
                }
			}
			else
			{
				db.ProductUnits.RemoveRange(productRemove.ProductUnits);
				db.Products.Remove(productRemove);
			}
			db.SaveChanges();
			_products.Remove(productRemove);
			if (productRawMaterialCombo.SelectedIndex == 0)
			{
				gridList = _products.Where(x => x.IsProduct).OrderBy(x => x.Name).ToList();
			}
			else
			{
				gridList = _products.Where(x => x.IsRawMaterial).OrderBy(x => x.Name).ToList();
			}
			Notify(nameof(gridList));
		}
		public void btnEdit_Click(object sender, RoutedEventArgs e)
		{
			SaveForm.Content = "Edit";
			Button edit = sender as Button;
			currentProduct = productGrid.SelectedItem as Product;
			NotifyAll();
		}
		private void productRawMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (productRawMaterialCombo.SelectedValue.ToString() == "ProductList")
			{
				gridList = _products.Where(x => x.IsProduct).ToList();
			}
			else
			{
				gridList = _products.Where(x => x.IsRawMaterial).ToList();
			}
			Notify(nameof(gridList));
		}

		private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
		{
			bool isProduct = (productRawMaterialCombo.SelectedValue.ToString() == "ProductList") ? true : false;
			if (!SearchBox.Text.Equals(""))
			{
				gridList = GlobalMethods.searchProduct(SearchBox.Text, gridColumns.SelectedValue.ToString(), _products, isProduct);
				Notify(nameof(gridList));
			}
		}

        private void productGrid_Loaded(object sender, RoutedEventArgs e)
        {
			productGrid.Height = SystemParameters.MaximizedPrimaryScreenHeight - productGrid.Margin.Top - GlobalMethods.MainFrameMargin - mainGrid.Margin.Bottom; 
        }
    }
}