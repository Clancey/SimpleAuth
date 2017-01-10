using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class ADFSApi : OAuthApi
	{
		private string AuthorizeUrl { get; set; }
		private string Resource { get; set; }
		private string RedirectUrl { get; set; }

		public ADFSApi(string identifier, string clientId, string authorizeUrl,
					   string tokenUrl, string resource, string redirectUrl = "http://localhost", HttpMessageHandler handler = null)
			: base(identifier, clientId, clientId, handler) // ADFS doesn't use client_secret (so just use client_id)
		{
			this.AuthorizeUrl = authorizeUrl;
			this.TokenUrl = tokenUrl;
			this.Resource = resource;
			this.RedirectUrl = redirectUrl;
		}

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new ADFSAuthenticator(ClientId, AuthorizeUrl, TokenUrl, Resource, RedirectUrl);
		}

		protected override async Task<Account> PerformAuthenticate()
		{
			var account = CurrentOAuthAccount ?? GetAccount<OAuthAccount>(Identifier);

			if (account != null && !string.IsNullOrWhiteSpace(account.RefreshToken))
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
				CurrentShowAuthenticator (authenticator);
			else
				ShowAuthenticator (authenticator);

			string token = await authenticator.GetAuthCode();

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
	}

	class ADFSAuthenticator : OAuthAuthenticator
	{
		private string Resource { get; set; }

		public ADFSAuthenticator(string clientId,
								 string authorizeUrl,
								 string tokenUrl,
								 string resource,
								 string redirectUrl) : base(authorizeUrl,
															tokenUrl,
															redirectUrl,
															clientId,
															null)
		{
			this.Resource = resource;
		}

		public override Dictionary<string, string> GetInitialUrlQueryParameters()
		{
			var parameters = base.GetInitialUrlQueryParameters();
			parameters["resource"] = Resource;
			return parameters;
		}

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["redirect_uri"] = RedirectUrl.OriginalString;
			return data;
		}
	}
}

