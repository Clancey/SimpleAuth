using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class FacebookApi : OAuthApi
	{
		public static bool IsUsingNative { get; set; }
		public static Action<WebAuthenticator> ShowFacebookAuthenticator { get; set; }
		public FacebookApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{
			InitFacebook();

			Scopes = new[] { "public_profile" };
			authenticator = new FacebookAuthenticator
			{
				ClientId = clientId,
				ClientSecret = clientSecret,
			};
		}
		public FacebookApi(string identifier, FacebookAuthenticator authenticator, HttpMessageHandler handler = null) : base(identifier, authenticator, handler)
		{
			InitFacebook();
			Scopes = authenticator.Scope?.ToArray() ?? new[] {"public_profile"};
		}
		private void InitFacebook()
		{
			BaseAddress = new Uri("https://graph.facebook.com");
			TokenUrl = "https://graph.facebook.com/v2.3/oauth/access_token?";

			CurrentShowAuthenticator = (a) =>
			{
				a.Cookies = CurrentOAuthAccount?.Cookies;
				if (ShowFacebookAuthenticator != null)
					ShowFacebookAuthenticator(a);
				else
					ShowAuthenticator(a);
			};
		}
		protected override WebAuthenticator CreateAuthenticator()
		{
			return base.CreateAuthenticator();
		}

		//Thanks you facebook for making your token exchange a get...
		protected override async Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
		{
			var auth = authenticator as FacebookAuthenticator;
			OauthResponse result;
			if (IsUsingNative)
			{
				result = new OauthResponse
				{
					ExpiresIn = auth.Expiration,
					TokenType = "Bearer",
					AccessToken = auth.AuthCode,
					RefreshToken = auth.AuthCode,
				};
			}
			else {
				var postData = await authenticator.GetTokenPostData(ClientSecret);
				if (string.IsNullOrWhiteSpace(TokenUrl))
					throw new Exception("Invalid TokenURL");
				var url = new Uri(TokenUrl).AddParameters(postData);
				var reply = await Client.GetAsync(url);
				var resp = await reply.Content.ReadAsStringAsync();
				result = Deserialize<OauthResponse>(resp);
				if (!string.IsNullOrEmpty(result.Error))
					throw new Exception(result.ErrorDescription);
			}

			var account = new OAuthAccount()
			{
				ExpiresIn = result.ExpiresIn,
				Created = DateTime.UtcNow,
				RefreshToken = result.RefreshToken,
				Scope = authenticator.Scope?.ToArray(),
				TokenType = result.TokenType,
				Token = result.AccessToken,
				ClientId = ClientId,
				Identifier = identifier,
			};
			return account;
		}
	}

	public class FacebookAuthenticator : OAuthAuthenticator
	{
		public override string BaseUrl { get; set; } = "https://m.facebook.com/dialog/oauth/";

		public override Uri RedirectUrl
		{
			get;
			set;
		} = new Uri("https://localhost");

		public override Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			return Task.FromResult(new Dictionary<string, string> {
				{
					"redirect_uri",
					RedirectUrl.ToString()
				}, {
					"code",
					AuthCode
				}, {
					"client_id",
					ClientId
				}, {
					"client_secret",
					clientSecret
				},
			});
		}
		public long Expiration { get; private set; }
		public void OnRecievedAuthCode(string authCode, long expiration)
		{
			Expiration = expiration;
			FoundAuthCode(authCode);
		}
		public override async Task<Uri> GetInitialUrl()
		{
			var url = await base.GetInitialUrl();
			return url;
		}
	}
}

