using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace BillMaker.LicenseArgs
{
	public class LicenseCheckingResponse
	{
		private String _xDate;
		public String ExpDate
		{ get { return _xDate; } }
		[JsonConstructor]
		internal LicenseCheckingResponse(String ExpDate)
		{
			_xDate = ExpDate;
		}
	}
}