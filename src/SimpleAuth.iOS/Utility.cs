using System;
using Foundation;
using Security;

namespace SimpleAuth
{
	public static class Utility
	{
		private static NSUserDefaults prefs = NSUserDefaults.StandardUserDefaults;

		static internal void SetSecured(string key, string value, string clientId, string service)
		{
			var s = new SecRecord(SecKind.GenericPassword)
			{
				Service = $"{clientId}-{key}-{service}",
			};

			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord(s, out res);
			if (res == SecStatusCode.Success)
			{
				var remStatus = SecKeyChain.Remove(s);
			}

			s.ValueData = NSData.FromString(value);
			var err = SecKeyChain.Add(s);
		}
		static internal string GetSecured(string id, string clientId, string service)
		{
			var rec = new SecRecord(SecKind.GenericPassword)
			{
				Service = $"{clientId}-{id}-{service}",
			};

			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord(rec, out res);
			if (res == SecStatusCode.Success)
				return match.ValueData.ToString();
			return "";
		}
	}
}

