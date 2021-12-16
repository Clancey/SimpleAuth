using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
	class AuthStorage : IAuthStorage
	{
		const string ResourceIdentifier = "SimpleAuth.AuthStorage";
		public string GetSecured(string id, string clientId, string service, string sharedGroup)
		{
			var vault = new Windows.Security.Credentials.PasswordVault();
			try
			{
				return vault.Retrieve(ResourceIdentifier, $"{clientId}-{id}-{service}").Password;
			}
			catch
			{
				return "";
			}
		}

		public void SetSecured(string id, string value, string clientId, string service, string sharedGroup)
		{
			var vault = new Windows.Security.Credentials.PasswordVault();
			try
			{
				var pass = vault.Retrieve(ResourceIdentifier, $"{clientId}-{id}-{service}");
                if (pass != null)
                {
                    vault.Remove(pass);

                    // This clears the WebView cache which is used for Instagram
                    #pragma warning disable 4014
                   //WebView2.ClearTemporaryWebDataAsync();
                    #pragma warning restore 4014
                }
            }
			catch { }
            if (!string.IsNullOrEmpty(value))
			    vault.Add(new Windows.Security.Credentials.PasswordCredential(ResourceIdentifier, $"{clientId}-{id}-{service}", value));
		}
	}
}
