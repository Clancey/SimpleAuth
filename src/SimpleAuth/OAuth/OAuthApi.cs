using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
	public class OAuthApi : AuthenticatedApi
	{
		
		static OAuthApi ()
		{
			//Setup default ShowAuthenticator
			#if __IOS__
			OAuthApi.ShowAuthenticator = (authenticator) => {
				var invoker = new Foundation.NSObject ();
				invoker.BeginInvokeOnMainThread (() => {
					var vc = new iOS.WebAuthenticatorViewController (authenticator);
					var window = UIKit.UIApplication.SharedApplication.KeyWindow;
					var root = window.RootViewController;
					if (root != null) {
						var current = root;
						while (current.PresentedViewController != null) {
							current = current.PresentedViewController;
						}
						current.PresentViewControllerAsync (new UIKit.UINavigationController (vc), true);
					}
				});
			};

			#elif __ANDROID__
			OAuthApi.ShowAuthenticator = (authenticator) =>
			{
				var context = Android.App.Application.Context;
				var i = new global::Android.Content.Intent(context, typeof(WebAuthenticatorActivity));
				var state = new WebAuthenticatorActivity.State
				{
					Authenticator = authenticator,
				};
				i.SetFlags(Android.Content.ActivityFlags.NewTask);
				i.PutExtra("StateKey", WebAuthenticatorActivity.StateRepo.Add(state));
				context.StartActivity(i);
			};
			#elif __OSX__
			OAuthApi.ShowAuthenticator = (authenticator) =>
			{
				var invoker = new Foundation.NSObject();
				invoker.BeginInvokeOnMainThread(() =>
				{
					var vc = new SimpleAuth.Mac.WebAuthenticatorWebView(authenticator);
					SimpleAuth.Mac.WebAuthenticatorWebView.ShowWebivew(vc);
				});
			};

            #elif WINDOWS_UWP
			OAuthApi.ShowAuthenticator = async (authenticator) =>
			{
				await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
				{
					var vc = new SimpleAuth.UWP.WebAuthenticatorWebView(authenticator);
					await vc.ShowAsync();
				});
			};

			#endif
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:SimpleAuth.OAuthApi"/> class.
		/// </summary>
		/// <param name="identifier">This is used to store and look up credentials/cookies for the API</param>
		/// <param name="clientId">OAuth Client identifier.</param>
		/// <param name="clientSecret">OAuth Client secret.</param>
		/// <param name="tokenUrl">URL for swaping out the token.</param>
		/// <param name="authorizationUrl">Login website URL.</param>
		/// <param name="redirectUrl">Redirect URL. Defaults to http://localhost</param>
		/// <param name="handler">Handler.</param>
		public OAuthApi(string identifier, string clientId, string clientSecret,string tokenUrl,string authorizationUrl,string redirectUrl = "http://localhost", HttpMessageHandler handler = null) : this(identifier, clientId, clientSecret, handler)
		{
			this.TokenUrl = tokenUrl;
			authenticator = new OAuthAuthenticator(authorizationUrl,tokenUrl,redirectUrl,clientId,clientSecret);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="T:SimpleAuth.OAuthApi"/> class.
		/// </summary>
		/// <param name="identifier">This is used to store and look up credentials/cookies for the API</param>
		/// <param name="authenticator">OAuth Authenticator.</param>
		/// <param name="handler">Handler.</param>
		public OAuthApi(string identifier, OAuthAuthenticator authenticator, HttpMessageHandler handler = null) : this(identifier, authenticator.ClientId, authenticator.ClientSecret, handler)
		{
			this.authenticator = authenticator;
			TokenUrl = authenticator.TokenUrl;
		}

		public bool ScopesRequired { get; set; } = true;

		public static Action<WebAuthenticator> ShowAuthenticator { get; set; }
		public Action<WebAuthenticator> CurrentShowAuthenticator { get; set; }
		protected OAuthApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base(identifier, clientSecret, handler)
		{
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
		}

		protected WebAuthenticator authenticator;
		public OAuthAccount CurrentOAuthAccount => CurrentAccount as OAuthAccount;

		public string TokenUrl { get; set; }

		public string[] Scopes { get; set; }

		public override void ResetData()
		{
			base.ResetData();
			if (authenticator != null)
				authenticator.ClearCookiesBeforeLogin = true;
		}
		public bool ForceRefresh { get; set; }
		protected override async Task<Account> PerformAuthenticate()
		{

			if (ScopesRequired && (Scopes?.Length ?? 0) == 0)
				throw new Exception("Scopes must be set on the API or passed into Authenticate");
			var account = CurrentOAuthAccount ?? GetAccount<OAuthAccount>(Identifier);
			if (account != null && (!string.IsNullOrWhiteSpace(account.RefreshToken) || account.ExpiresIn <= 0))
			{
				var valid = account.IsValid();
				if (!valid || ForceRefresh)
				{
					if (!(await Ping(TokenUrl)))
						return account;
					await RefreshAccount(account);
				}

				if (account.IsValid())
				{
					SaveAccount(account);
					CurrentAccount = account;
					return account;
				}
			}

			authenticator = CreateAuthenticator();
			authenticator.Cookies = account?.Cookies;
			await authenticator.PrepareAuthenticator ();

			if (CurrentShowAuthenticator != null)
				CurrentShowAuthenticator(authenticator);
			else
				ShowAuthenticator(authenticator);

			var token = await authenticator.GetAuthCode();

			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Null token");
			}
			account = await GetAccountFromAuthCode(authenticator, Identifier);
			account.Identifier = Identifier;
			SaveAccount(account);
			CurrentAccount = account;
			return account;
		}

		protected virtual async Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
		{
			var postData = await authenticator.GetTokenPostData(ClientSecret);
			if (string.IsNullOrWhiteSpace(TokenUrl))
				throw new Exception("Invalid TokenURL");
			var message = new HttpRequestMessage (HttpMethod.Post, TokenUrl) {
				Content = new FormUrlEncodedContent (postData),
				Headers = {
					{"Accept","application/json"}
				}
			};
			var reply = await Client.SendAsync (message);
			var resp = await reply.Content.ReadAsStringAsync();
			var result = Deserialize<OauthResponse>(resp);
			if (!string.IsNullOrEmpty(result?.Error))
 				throw new Exception(result.ErrorDescription);

			var account = new OAuthAccount () {
				ExpiresIn = result.ExpiresIn,
				Created = DateTime.UtcNow,
				RefreshToken = result.RefreshToken,
				Scope = authenticator.Scope?.ToArray (),
				TokenType = result.TokenType,
				Token = result.AccessToken,
				ClientId = ClientId,
				Identifier = identifier,
				Cookies = authenticator.Cookies,
			};
			return account;
		}

		protected virtual WebAuthenticator CreateAuthenticator()
		{
			authenticator.Scope = Scopes?.ToList();
			authenticator.Cookies = CurrentOAuthAccount?.Cookies;
			return authenticator;
		}
		protected async Task<bool> RefreshToken(Account accaccount)
		{
			try
			{
				var account = accaccount as OAuthAccount;
				if (account == null)
					throw new Exception("Invalid Account");
				var message = new HttpRequestMessage (HttpMethod.Post, TokenUrl) {
					Content = new FormUrlEncodedContent (new Dictionary<string, string>
					{
						{"grant_type","refresh_token"},
						{"refresh_token",account.RefreshToken},
						{"client_id",ClientId},
						{"client_secret",ClientSecret},
					}),
					Headers = {
					{"Accept","application/json"}
				}
				};
				var reply = await Client.SendAsync (message);
				var resp = await reply.Content.ReadAsStringAsync();
				var result = Deserialize<OauthResponse>(resp);
				if (!string.IsNullOrEmpty(result.Error))
				{
					if (string.IsNullOrWhiteSpace(account.RefreshToken) || result.Error == "invalid_grant" || result.ErrorDescription.IndexOf("revoked", StringComparison.CurrentCultureIgnoreCase) >= 0)
					{
						account.Token = "";
						account.RefreshToken = "";
						account.ExpiresIn = 1;
						SaveAccount(account);
						return await PerformAuthenticate () != null;
					}
					else
						throw new Exception(result.ErrorDescription);
				}
				if (!string.IsNullOrEmpty(result.RefreshToken))
					account.RefreshToken = result.RefreshToken;
				account.TokenType = result.TokenType;
				account.Token = result.AccessToken;
				account.ExpiresIn = result.ExpiresIn;
				account.Created = DateTime.UtcNow;
				if (account == CurrentAccount)
					await OnAccountUpdated(account);
				CurrentAccount = account;
				SaveAccount(account);
				return true;
			}
			catch (Exception ex)
			{
				OnException(this, ex);
			}
			return false;
		}

		Task<bool> refreshTask;
		object locker = new object();
		protected override async Task<bool> RefreshAccount(Account account)
		{
			lock (locker) {
				if (refreshTask == null || refreshTask.IsCompleted)
					refreshTask = RefreshToken (account);
			}
			return await refreshTask;
		}

		public override async Task PrepareClient(HttpClient client)
		{
			await base.PrepareClient(client);

			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(CurrentOAuthAccount.TokenType, CurrentOAuthAccount.Token);
		}
	}

}
