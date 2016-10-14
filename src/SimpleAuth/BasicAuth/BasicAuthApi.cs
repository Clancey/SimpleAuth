using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
    public class BasicAuthApi : AuthenticatedApi
	{
		protected string LoginUrl { get; set; }
		static BasicAuthApi()
		{

			//Setup default ShowAuthenticator
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

		/// <summary>
		/// Initializes a new instance of the <see cref="T:SimpleAuth.BasicAuthApi"/> class.
		/// </summary>
		/// <param name="identifier">This is used to store and look up credentials/cookies for the API</param>
		/// <param name="encryptionKey">Encryption key used to store information.</param>
		/// <param name="loginUrl">Login URL for the credentials to be tested against.</param>
		/// <param name="handler">Handler.</param>
	    public BasicAuthApi(string identifier,string encryptionKey,string loginUrl, HttpMessageHandler handler = null) : base(identifier,encryptionKey, handler)
	    {
		    LoginUrl = loginUrl;
			authenticator = new BasicAuthAuthenticator(Client,loginUrl);
			ClientSecret = encryptionKey;
			ClientId = identifier;

	    }

	    protected BasicAuthAuthenticator authenticator;

		protected virtual BasicAuthAuthenticator CreateAuthenticator()
		{
			return authenticator;
		}

		public BasicAuthAccount CurrentBasicAccount => CurrentAccount as BasicAuthAccount;
		public static Action<BasicAuthAuthenticator> ShowAuthenticator { get; set; }
		public Action<BasicAuthAuthenticator> CurrentShowAuthenticator { get; set; }
		protected override async Task<Account> PerformAuthenticate()
	    {
			var account = CurrentBasicAccount ?? GetAccount<BasicAuthAccount>(Identifier);
			if (account?.IsValid() == true)
		    {
			    return CurrentAccount = account;
		    }

			authenticator = CreateAuthenticator();

			if(CurrentShowAuthenticator != null)
				CurrentShowAuthenticator(authenticator);
			else
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

	    protected override Task<bool> RefreshAccount(Account account)
	    {
			//This should never be called. Basic auth never expires;
			return Task.FromResult(true);
	    }

		public override async Task PrepareClient (HttpClient client)
		{
			await base.PrepareClient (client);
			Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue ("Basic", CurrentBasicAccount?.Key);
		}

	}
}
