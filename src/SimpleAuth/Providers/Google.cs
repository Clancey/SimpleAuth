using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleAuth.Providers
{
	public class GoogleApi : OAuthApi
	{

		/// <summary>
		/// Only use this Constructor for platofms using NativeAuth
		/// </summary>
		/// <param name="identifier">Identifier.</param>
		/// <param name="clientId">Client identifier.</param>
		/// <param name="handler">Handler.</param>
		public GoogleApi(string identifier, string clientId, HttpMessageHandler handler = null) : this(identifier, clientId, "native", handler)
		{
			
		}
		public GoogleApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base(identifier, CleanseClientId(clientId), clientSecret, handler)
		{
			this.TokenUrl = "https://accounts.google.com/o/oauth2/token";
#if __UNIFIED__
			this.CurrentShowAuthenticator = NativeSafariAuthenticator.ShowAuthenticator;
			NativeSafariAuthenticator.RegisterCallbacks ();
#endif
			if (GoogleShowAuthenticator != null)
				CurrentShowAuthenticator = GoogleShowAuthenticator;
		}

		public static Action<WebAuthenticator> GoogleShowAuthenticator { get; set; }
		public static string CleanseClientId (string clientId) => clientId?.Replace (".apps.googleusercontent.com", "");

        public static bool IsUsingNative { get; set; }
		public Uri RedirectUrl { get; set; } = new Uri("http://localhost");

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new GoogleAuthenticator {
				Scope = Scopes.ToList(),
				ClientId = ClientId,
				ClearCookiesBeforeLogin = CalledReset,
				RedirectUrl = RedirectUrl,
				IsUsingNative = IsUsingNative,
			};
		}

        protected override Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
        {
            var auth = authenticator as GoogleAuthenticator;
			if (IsUsingNative) {
				return Task.FromResult (new OAuthAccount () {
					ExpiresIn = 3600,
					Created = DateTime.UtcNow,
					Scope = authenticator.Scope?.ToArray (),
					TokenType = "Bearer",
					Token = auth.AuthCode,
					ClientId = ClientId,
					Identifier = identifier,
				});
			}

            return base.GetAccountFromAuthCode(authenticator, identifier);
        }

		public async Task<GoogleUserProfile> GetUserInfo(bool forceRefresh = false)
		{
			string userInfoJson;
			if(forceRefresh || !CurrentAccount.UserData.TryGetValue("userInfo", out userInfoJson))
			{
				CurrentAccount.UserData["userInfo"] =
					userInfoJson = await Get("https://www.googleapis.com/oauth2/v1/userinfo?alt=json");
				SaveAccount(CurrentAccount);
			}

			return Deserialize<GoogleUserProfile>(userInfoJson);

		}
	}

	public class GoogleAuthenticator : OAuthAuthenticator
	{ 
		public override string BaseUrl
		{
			get;
			set;
		} = "https://accounts.google.com/o/oauth2/auth";

		public override Uri RedirectUrl
		{
			get;
			set;
		} 

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["scope"] = string.Join(" ", Scope);
			data ["client_id"] = GetGoogleClientId (ClientId);
			if (data["client_secret"] == "native")
				data.Remove("client_secret");
			data["redirect_uri"] = GetRedirectUrl();
			return data;
		}
		public override Dictionary<string, string> GetInitialUrlQueryParameters ()
		{
			var data = base.GetInitialUrlQueryParameters ();
			data ["redirect_uri"] = GetRedirectUrl ();
			data ["client_id"] = GetGoogleClientId (ClientId);
			return data;
		}

		public static string GetGoogleClientId (string clientId) => $"{clientId}.apps.googleusercontent.com";

		public bool IsUsingNative { get; set; }
		public virtual string GetRedirectUrl ()
		{
			if (IsUsingNative)
				return "";
			//Only implemented for iOS/mac right now
#if __UNIFIED__
			//for google, the redirect is a reverse of the client ID
			return $"com.googleusercontent.apps.{ClientId}:/oauthredirect";
#elif WINDOWS_UWP
            return "urn:ietf:wg:oauth:2.0:oob";
#endif
			return RedirectUrl.AbsoluteUri;
		}

		public override bool CheckUrl (Uri url, Cookie [] cookies)
		{
			try {
				if (url == null || string.IsNullOrWhiteSpace (url.Query))
					return false;
				var parts = HttpUtility.ParseQueryString (url.Query);
				var code = parts ["code"];
				if (!string.IsNullOrWhiteSpace (code)) {
					Cookies = cookies?.Select (x => new CookieHolder { Domain = x.Domain, Path = x.Path, Name = x.Name, Value = x.Value }).ToArray ();
					FoundAuthCode (code);
					return true;
				}

			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}


		public static string UrlFromClientId (string clientId)
		{
			var parts = clientId.Split ('.');
			return string.Join (".", parts.Reverse ());
		}


		public override async Task<Uri> GetInitialUrl()
		{
			var uri = await base.GetInitialUrl();
			return new Uri(uri.AbsoluteUri + "&access_type=offline");
		}

        public void OnRecievedAuthCode(string authCode)
        {
            FoundAuthCode(authCode);
        }
	}


	public class GoogleUserProfile
	{
		[JsonProperty("id")]
		public string Id
		{
			get;
			set;
		}

		[JsonProperty("email")]
		public string Email
		{
			get;
			set;
		}

		[JsonProperty("verified_email")]
		public bool VerifiedEmail
		{
			get;
			set;
		}

		[JsonProperty("name")]
		public string Name
		{
			get;
			set;
		}

		[JsonProperty("given_name")]
		public string GivenName
		{
			get;
			set;
		}

		[JsonProperty("family_name")]
		public string FamilyName
		{
			get;
			set;
		}

		[JsonProperty("link")]
		public string Link
		{
			get;
			set;
		}

		[JsonProperty("picture")]
		public string Picture
		{
			get;
			set;
		}

		[JsonProperty("gender")]
		public string Gender
		{
			get;
			set;
		}

		[JsonProperty("locale")]
		public string Locale
		{
			get;
			set;
		}

		[JsonProperty("hd")]
		public string Hd
		{
			get;
			set;
		}
	}

}
