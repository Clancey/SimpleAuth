using System;
namespace SimpleAuth
{
	public interface IAuthStorage
	{
		void SetSecured (string identifier, string value, string clientId, string clientSecret, string sharedGroup);
        string GetSecured (string identifier, string clientId, string clientSecret, string sharedGroup);
	}
}

