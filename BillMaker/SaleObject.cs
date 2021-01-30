using BillMaker.DataConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillMaker
{
	public class SaleObject
	{
		public Sale sale;
		
		public SaleObject()
		{
			sale = new Sale();
		}
	}
}
