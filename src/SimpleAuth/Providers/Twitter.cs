using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class TwitterApi : OAuthApi
	{
		public TwitterApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{
			ScopesRequired = false;
			BaseAddress = new Uri("https://api.twitter.com/1.1/");
			TokenUrl = "https://api.twitter.com/oauth2/token";
		}
		public Uri RedirectUrl { get; set; } = new Uri("https://google.com");
		protected override WebAuthenticator CreateAuthenticator()
		{
			var ta = authenticator as TwitterAuthenticator ?? new TwitterAuthenticator
			{
				Api = this,
				TokenUrl = TokenUrl,
				ClientId = ClientId,
				ClientSecret = ClientSecret,
				RedirectUrl = RedirectUrl,
			};
			return authenticator = ta;
		}

		protected override async Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
		{
			var ta = authenticator as TwitterAuthenticator;
			OauthToken = ta.AuthCode;
			var resp = await PostMessage("https://api.twitter.com/oauth/access_token", new FormUrlEncodedContent(new Dictionary<string, string> { { "oauth_verifier", ta.CodeVerifier } }), authenticated: false);
			var data = HttpUtility.ParseQueryString(await resp.Content.ReadAsStringAsync());
			var account = new TwitterAccount()
			{
				ExpiresIn = 0,
				Created = DateTime.UtcNow,
				RefreshToken = ta.AuthCode,
				Scope = authenticator.Scope?.ToArray(),
				TokenType = "Oauth",
				Token = data["oauth_token"],
				OAuthSecret = data["oauth_token_secret"],
				ClientId = ClientId,
				Identifier = identifier,
			};
			return account;

		}

		protected override T GetAccount<T>(string identifier)
		{
			//Yes this is ugly, but we wan it to be TwitterAccount
			return (T)((object)base.GetAccount<TwitterAccount>(identifier));
		}
		string OauthToken = "";
		public override async System.Threading.Tasks.Task<HttpResponseMessage> SendMessage(HttpRequestMessage message, bool authenticated = true, HttpCompletionOption completionOption = 0)
		{
			var uri = message.RequestUri;
			var contentT = (message?.Content as FormUrlEncodedContent)?.ReadAsStringAsync();
			string content = "";
			if (contentT != null)
				content = await contentT;

			var timestamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
			var headers = new SortedDictionary<string, string>
			{
				{"oauth_consumer_key" , ClientId},
				{"oauth_nonce", Guid.NewGuid().ToString()},
				{"oauth_signature_method" , "HMAC-SHA1"},
				{"oauth_timestamp" , timestamp},
				{"oauth_version" , "1.0"}
			};
			var token = CurrentOAuthAccount?.Token ?? OauthToken;
			if (!string.IsNullOrWhiteSpace(token))
				headers.Add("oauth_token", token);
			var keyValues = new SortedDictionary<string, string>(headers);
			if (!string.IsNullOrWhiteSpace(content))
			{
				var data = HttpUtility.ParseQueryString(content);
                //PCLs have a different return type for HttpUtility. For shame...
#if __PCL__ || WINDOWS_UWP || NETSTANDARD1_4
                foreach (var k in data)
				{
					keyValues[k.Key] = data[k.Value];
				}

#else
				foreach (var k in data?.AllKeys)
				{
					keyValues[k] = data[k];
				}

#endif
			}
			var param = HttpUtility.ParseQueryString(uri.Query);
#if __PCL__ || WINDOWS_UWP || NETSTANDARD1_4
            foreach (var k in param)
			{
				keyValues[k.Key] = param[k.Value];
			}

#else
			foreach (var k in param?.AllKeys)
			{
				keyValues[k] = param[k];
			}

#endif

			var hash = ComputeHash(message.Method, uri, keyValues, ClientSecret, authenticated ? (CurrentAccount as TwitterAccount).OAuthSecret : "");

			headers.Add("oauth_signature", hash);
			var auth = string.Join(",", headers.Select(x => $"{x.Key}=\"{Uri.EscapeDataString(x.Value)}\""));
			message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", auth);

			return await Client.SendAsync(message, completionOption);
		}
		public override Task PrepareClient(HttpClient client)
		{
			return Task.FromResult(true);
		}
		public static string ComputeHash(HttpMethod method, Uri url, SortedDictionary<string, string> keyValues, string clientSecret, string appSecret)
		{
			var sig = GetSignature(method.ToString().ToUpper(), url, keyValues, clientSecret, appSecret);
			return sig;
		}

		public static string GetSignature(string method, Uri uri, IDictionary<string, string> parameters, string clientSecret, string tokenSecret)
		{
			var stringBuilder = new StringBuilder($"{method.ToUpper()}&{Uri.EscapeDataString(uri.AbsoluteUri)}&");
			foreach (var keyValuePair in parameters)
			{
				stringBuilder.Append(Uri.EscapeDataString($"{keyValuePair.Key}={keyValuePair.Value}&"));
			}
			string signatureBaseString = stringBuilder.ToString().Substring(0, stringBuilder.Length - 3);

			string signatureKey = $"{Uri.EscapeDataString(clientSecret)}&{Uri.EscapeDataString(tokenSecret)}";
            var hmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(signatureKey));


            string signatureString = Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString)));

			return signatureString;
		}
	}

	public class TwitterAuthenticator : OAuthAuthenticator
	{
		public TwitterAuthenticator()
		{
			BaseUrl = "https://api.twitter.com/oauth/authenticate";
			RedirectUrl = new Uri("http://localhost");
		}

		WeakReference api;
		public TwitterApi Api
		{
			get { return api?.Target as TwitterApi; }
			set { api = new WeakReference(value); }
		}

		public override async System.Threading.Tasks.Task<Uri> GetInitialUrl()
		{
			var m = await Api.PostMessage("https://api.twitter.com/oauth/request_token", null, false);
			var s = await m.Content.ReadAsStringAsync().ConfigureAwait(false);
			var d = HttpUtility.ParseQueryString(s);
			var u = new Uri(BaseUrl).AddParameters("oauth_token", d["oauth_token"]);
			return u;
		}
		public string CodeVerifier { get; set; }
		public override bool CheckUrl(Uri url, System.Net.Cookie[] cookies)
		{
			try
			{
				if (url == null || string.IsNullOrWhiteSpace(url.Query))
					return false;
				if (url.Host != RedirectUrl.Host)
					return false;
				var parts = HttpUtility.ParseQueryString(url.Query);
				var code = parts["oauth_token"];
				if (!string.IsNullOrWhiteSpace(code) && TokenTask != null)
				{
					CodeVerifier = parts["oauth_verifier"];
					FoundAuthCode(code);
					return true;
				}

			}
			catch (Exception ex)
			{
				Api?.OnException(this, ex);
			}
			return false;
		}
	}

	public class TwitterAccount : OAuthAccount
	{
		public string OAuthSecret { get; set; }

		public override bool IsValid()
		{
			return !string.IsNullOrWhiteSpace(OAuthSecret) && !string.IsNullOrWhiteSpace(Token);
		}
	}
}

