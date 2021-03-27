using System;

namespace BillMaker.LicenseArgs
{
	public class LicenseVerifyRequest
	{
		private String _email;
		private String _password;
		private String _mKey;
		private String _sKey;

		public String Email
		{ get { return _email; } }
		public String Password
		{ get { return _password; } }
		public String MKey
		{ get { return _mKey; } }
		public String SKey
		{ get { return _sKey; } }

		public LicenseVerifyRequest(String Email, String Password, String MKey, String SKey)
		{
			_email = Email;
			_password = Password;
			_mKey = MKey;
			_sKey = SKey;
		}
	}
}