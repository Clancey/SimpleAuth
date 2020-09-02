using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
#if __IOS__
using Foundation;
using UIKit;
#endif

namespace SimpleAuth {

	public class OAuthPasswordApi : OAuthApi {
		public OAuthPasswordApi (string identifier, string clientId, string clientSecret, string serverUrl, string tokenUrl, string refreshUrl) : base (identifier, clientId, clientSecret, tokenUrl, refreshUrl)
		{
			BaseAddress = new Uri (serverUrl);
			Scopes = new [] { "null" };

			CurrentShowAuthenticator = (WebAuthenticator obj) => {
				var auth = obj as IBasicAuthenicator;
				if (auth != null)
					BasicAuthApi.ShowAuthenticator (auth);
			};
		}

		protected override WebAuthenticator CreateAuthenticator ()
		{
			return authenticator as OAuthPasswordAuthenticator ?? (authenticator = new OAuthPasswordAuthenticator {
				BaseUrl = TokenUrl,
				Api = this,

			});
		}
		protected override Task<OAuthAccount> GetAccountFromAuthCode (WebAuthenticator authenticator, string identifier)
		{
			var auth = authenticator as OAuthPasswordAuthenticator;
			var account = new OAuthAccount () {
				ExpiresIn = auth.Token.ExpiresIn,
				Created = DateTime.UtcNow,
				RefreshToken = auth.Token.RefreshToken,
				Scope = authenticator.Scope?.ToArray (),
				TokenType = auth.Token.TokenType,
				Token = auth.Token.AccessToken,
				ClientId = ClientId,
				Identifier = identifier,
			};
			return Task.FromResult (account);
		}
	}

	public class OAuthPasswordAuthenticator : WebAuthenticator, IBasicAuthenicator {
		public override string BaseUrl { get; set; }

		public override Uri RedirectUrl { get; set; }

		public AuthTokenClass Token { get; set; }

		WeakReference api;

		public OAuthPasswordApi Api {
			get {
				return api?.Target as OAuthPasswordApi;
			}

			set {
				api = new WeakReference (value);
			}
		}

		public virtual async Task<bool> VerifyCredentials (string username, string password)
		{
			Api.EnsureApiStatusCode = false;
			var tokenString = await Api.Post (new FormUrlEncodedContent (new Dictionary<string, string> {
				{ "username", username},
				{"password",password},
				{"grant_type","password"}
			}), BaseUrl, authenticated: false).ConfigureAwait (false);
			var token = tokenString.ToObject<AuthTokenClass> ();
			if (!string.IsNullOrEmpty (token.AccessToken)) {
				Token = token;
				this.FoundAuthCode (token.AccessToken);
				return true;
			}
			return false;
		}
		public class AuthTokenClass {

			[JsonProperty ("access_token")]
			public string AccessToken { get; set; }

			[JsonProperty ("token_type")]
			public string TokenType { get; set; }

			[JsonProperty ("expires_in")]
			public int ExpiresIn { get; set; }

			[JsonProperty ("refresh_token")]
			public string RefreshToken { get; set; }

			[JsonProperty ("userName")]
			public string UserName { get; set; }

			[JsonProperty (".issued")]
			public DateTime Issued { get; set; }

			[JsonProperty (".expires")]
			public DateTime Expires { get; set; }

			public string Error { get; set; }
		}


		public Task<bool> SignUp (string username, string password)
		{
			return Task.FromResult (false);
		}
	}
}

