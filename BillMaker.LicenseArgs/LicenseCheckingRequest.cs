using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMaker.LicenseArgs
{
	public class LicenseCheckingRequest
	{
		private String _email;
		private String _mKey;
		public String Email
		{ get { return _email; } }
		public String MKey
		{ get { return _mKey; } }
		public LicenseCheckingRequest(String email, String mKey)
		{
			_email = email;
			_mKey = mKey;
		}
	}
}