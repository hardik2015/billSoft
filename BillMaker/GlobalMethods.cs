using BillMaker.DataConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillMaker
{
	
	public enum SaleTypes
	{
		BuyType = 0,
		SellType = 1
	}
	public class GlobalMethods
	{

		static MyAttachedDbEntities db = new MyAttachedDbEntities();
		public static String companyName;
		public static String HsnNo;
		public static String Email;
		public static String phoneNo;

		public static void LoadCompanyDetails()
        {
			companyName = db.CompanySettings.Where(x => x.Name == "CompanyName").FirstOrDefault().Value;
			HsnNo = db.CompanySettings.Where(x => x.Name == "HSNNo").FirstOrDefault().Value;
			Email = db.CompanySettings.Where(x => x.Name == "CompanyEmailId").FirstOrDefault().Value;
			phoneNo = db.CompanySettings.Where(x => x.Name == "CompanyPhone").FirstOrDefault().Value;
		}
		public static List<Product> searchProduct(string Searchstring, string columnType, List<Product> productSource, bool IsRawMaterial)
		{
			List<Product> list = new List<Product>();
			IEnumerable<Product> query = new List<Product>();
			if (columnType == "Name")
			{
				query = (from product in productSource
						 where product.Name.Contains(Searchstring)
						 select product);
			}
			else if (columnType == "Cgst")
			{
				Decimal cgstValue = 0;
				if (!Decimal.TryParse(Searchstring, out cgstValue))
				{
					MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error!!", "Given Cgst value :" + Searchstring + " is Inavlid");
					_ = messageBoxDialog.ShowAsync();
					return list;
				}
				query = (from product in productSource
						 where product.Cgst == cgstValue
						 select product);
			}
			else if (columnType == "Sgst")
			{
				Decimal sgstValue = 0;
				if (!Decimal.TryParse(Searchstring, out sgstValue))
				{
					MessageBoxDialog messageBoxDialog = new MessageBoxDialog("Error!!", "Given Cgst value :" + Searchstring + " is Inavlid");
					_ = messageBoxDialog.ShowAsync();
					return list;
				}
				query = (from product in productSource
						 where product.Cgst == sgstValue
						 select product);
			}
			else if (columnType == "Basic Unit")
			{
				query = (from product in productSource
						 where product.MeasureUnit.UnitName.Contains(Searchstring)
						 select product);
			}

			if(IsRawMaterial)
			{
				list = query.Where(x => x.IsRawMaterial).ToList();
			}
			else
			{
				list = query.Where(x => x.IsProduct).ToList();
			}
			return list;
		}

		public static List<Person> searchPerson(string Searchstring, string columnType, List<Person> peopleSource, bool IsVendor)
		{
			List<Person> list = new List<Person>();
			IEnumerable<Person> query = new List<Person>();
			if (columnType == "Name")
			{
				query = (from person in peopleSource
						 where person.PersonName.Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Email")
			{
				query = (from person in peopleSource
						 where person.Email.Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Phone No")
			{
				query = (from person in peopleSource
						 where person.Phone.Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Address")
			{
				query = (from person in peopleSource
						 where person.Address.Contains(Searchstring)
						 select person);
			}
			else if (columnType == "City")
			{
				query = (from person in peopleSource
						 where person.City.Contains(Searchstring)
						 select person);
			}
			else if (columnType == "State")
			{
				query = (from person in peopleSource
						 where person.State.Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Country")
			{
				query = (from person in peopleSource
						 where person.Country.Contains(Searchstring)
						 select person);
			}

			if (IsVendor)
			{
				list = query.Where(x => x.IsVendor).ToList();
			}
			else
			{
				list = query.Where(x => x.IsCustomer).ToList();
			}
			return list;
		}
	}
}
