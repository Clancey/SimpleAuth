using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
    public class BasicAuthApi : Api
    {
		protected string LoginUrl { get; set; }
	    public BasicAuthApi(string identifier,string loginUrl, HttpMessageHandler handler = null) : base(identifier, handler)
	    {
		    LoginUrl = loginUrl;
			authenticator = new BasicAuthAuthenticator(Client,loginUrl);

#if __IOS__
		    ShowAuthenticator = (auth) =>
		    {
				var invoker = new Foundation.NSObject();
				invoker.BeginInvokeOnMainThread(() =>
				{
					var controller = new BasicAuthController(auth);
					controller.Show();
				});
				
            };
#endif
	    }

	    protected BasicAuthAuthenticator authenticator;

		protected virtual BasicAuthAuthenticator CreateAuthenticator()
		{
			return authenticator;
		}

		public BasicAuthAccount CurrentBasicAccount => CurrentAccount as BasicAuthAccount;
		public static Action<BasicAuthAuthenticator> ShowAuthenticator { get; set; }
		protected override async Task<Account> PerformAuthenticate()
	    {
			var account = CurrentBasicAccount ?? GetAccount<BasicAuthAccount>(Identifier);
			if (account?.IsValid() == true)
		    {
			    return account;
		    }

			authenticator = CreateAuthenticator();

			ShowAuthenticator(authenticator);

			var token = await authenticator.GetAuthCode();
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Null token");
			}
			account = new BasicAuthAccount {Key = token};
			account.Identifier = Identifier;
			SaveAccount(account);
			CurrentAccount = account;
			return account;
		}

	    protected override async Task<bool> RefreshAccount(Account account)
	    {
			//This should never be called. Basic auth never expires;
		    return true;
	    }

	}
}
