using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
	static class Utility
	{
		const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform (iOS/Android) version";
		public static void SetSecured(string identifier, string value, string clientId, string clientSecret,string sharedGroup)
		{
			throw new Exception(wrongVersion);
		}

		public static string GetSecured(string identifier, string clientId, string clientSecret,string sharedGroup)
		{
			throw new Exception(wrongVersion);
		}
	}
}
