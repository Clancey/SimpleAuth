using System;
using System.Collections.Generic;

namespace SimpleAuth.Tests
{
	public class InMemoryAuthStorage : IAuthStorage
	{
		public InMemoryAuthStorage ()
		{
		}

		Dictionary<string, string> data = new Dictionary<string, string> ();
		public string GetSecured (string identifier, string clientId, string clientSecret, string sharedGroup)
		{
			string item;
			if (data.TryGetValue (GetId (identifier, clientId, clientSecret, sharedGroup), out item))
				return item;
			return null;
		}

		public void SetSecured (string identifier, string value, string clientId, string clientSecret, string sharedGroup)
		{
			data [GetId(identifier,clientId,clientSecret,sharedGroup)] = value;
		}

		string GetId (string identifier, string clientid, string clientSecret, string groupID)
		=> $"{identifier} - {clientid} - {clientSecret} - {groupID}";


		public void Reset ()
		{
			data.Clear ();
		}
	}
}
