using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
	class AuthStorage : IAuthStorage
	{
		const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform version or register a custom IAUthStorage";
		public void SetSecured(string identifier, string value, string clientId, string clientSecret,string sharedGroup)
		{
			throw new Exception(wrongVersion);
		}

		public string GetSecured(string identifier, string clientId, string clientSecret,string sharedGroup)
		{
			throw new Exception(wrongVersion);
		}
	}
}
