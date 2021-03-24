using BillMaker.DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillMaker
{
	
	public class GlobalMethods
	{

		static BillMakerEntities db = new BillMakerEntities();
		public static String companyName;
		public static String companyAddress;
		public static String GstINNo;
		public static String Email;
		public static String phoneNo;
		public static String TANNo;
		public static String BankAccountNumber;
		public static String BankIFSCCode;
		public static bool IsBankDetailsVisible;
		public static String settingDefaultPrinter;
		public static double MainFrameMargin;


		public static void LoadCompanyDetails()
        {
			companyName = db.CompanySettings.Where(x => x.Name == "CompanyName").FirstOrDefault().Value;
			GstINNo = db.CompanySettings.Where(x => x.Name == "CompanyGSTINNo").FirstOrDefault().Value;
			Email = db.CompanySettings.Where(x => x.Name == "CompanyEmailId").FirstOrDefault().Value;
			phoneNo = db.CompanySettings.Where(x => x.Name == "CompanyPhone").FirstOrDefault().Value;
			TANNo = db.CompanySettings.Where(x => x.Name == "CompanyTANNo").FirstOrDefault().Value;
			BankAccountNumber = db.CompanySettings.Where(x => x.Name == "CompanyAccountNumber").FirstOrDefault().Value;
			BankIFSCCode = db.CompanySettings.Where(x => x.Name == "ComapnyIFSCCode").FirstOrDefault().Value;
			IsBankDetailsVisible =  Int32.Parse( db.CompanySettings.Where(x => x.Name == "IsShowBankDetails").FirstOrDefault().Value) == 1;
			settingDefaultPrinter = db.CompanySettings.Where(x => x.Name == "DefaultPrinter").FirstOrDefault().Value;
			companyAddress = db.CompanySettings.Where(x => x.Name == "CompanyAddress").FirstOrDefault().Value;
		}
		public static List<Product> searchProduct(string Searchstring, string columnType, List<Product> productSource, bool IsProduct)
		{
			Searchstring = Searchstring.ToUpperInvariant();
			List<Product> list = new List<Product>();
			IEnumerable<Product> query = new List<Product>();
			if (columnType == "Name")
			{
				query = (from product in productSource
						 where product.Name.ToUpperInvariant().Contains(Searchstring)
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

			if(IsProduct)
			{
				list = query.Where(x => x.IsProduct).ToList();
			}
			else
			{
				list = query.Where(x => x.IsRawMaterial).ToList();
			}
			return list;
		}

		public static IEnumerable<Person> searchPerson(string Searchstring, string columnType, List<Person> peopleSource, bool IsCustomer)
		{
			Searchstring = Searchstring.ToUpperInvariant();
			IEnumerable<Person> query = new List<Person>();
			if(Searchstring.Equals(""))
            {

            }
			if (columnType == "Name")
			{
				query = (from person in peopleSource
						 where person.PersonName.ToUpperInvariant().Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Email")
			{
				query = (from person in peopleSource
						 where person.Email.ToUpperInvariant().Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Phone No")
			{
				query = (from person in peopleSource
						 where person.Phone.ToUpperInvariant().Contains(Searchstring)
						 select person);
			}
			else if (columnType == "Address")
			{
				query = (from person in peopleSource
						 where person.Address.ToUpperInvariant().Contains(Searchstring)
						 select person);
			}

			if (IsCustomer)
			{
				query = query.Where(x => x.IsCustomer);
			}
			else
			{
				query = query.Where(x => x.IsVendor);
			}
			return query;
		}
	}
}
