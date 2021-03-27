using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMaker.LicenseArgs
{
	public class LicenseVerifyResponse
	{

		private String _productKey;
		private String _expiryDate;

		public String ProductKey
		{ get { return _productKey; } }

		public String ExpiryKey
		{ get { return _expiryDate; } }

		[JsonConstructor]
		internal LicenseVerifyResponse(String ProductKey, String ExpiryKey)
		{
			_productKey = ProductKey;
			_expiryDate = ExpiryKey;
		}
	}
}