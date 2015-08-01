using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleAuth.Providers
{
	public class GoogleApi : OAuthApi
	{
		public GoogleApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{
			this.TokenUrl = "https://accounts.google.com/o/oauth2/token";
		}

		protected override Authenticator CreateAuthenticator(string[] scope)
		{
			return new GoogleAuthenticator
			{
				Scope = scope.ToList(),
				ClientId = ClientId,
			};
		}

		public async Task<GoogleUserProfile> GetUserInfo(bool forceRefresh = false)
		{
			string userInfoJson;
			if (forceRefresh || !CurrentAccount.UserData.TryGetValue("userInfo", out userInfoJson))
			{
				CurrentAccount.UserData["userInfo"] =
					userInfoJson = await GetString("https://www.googleapis.com/oauth2/v1/userinfo?alt=json");
				SaveAccount(CurrentAccount);
			}

			return Deserialize<GoogleUserProfile>(userInfoJson);

		}
	}

	class GoogleAuthenticator : Authenticator
	{
		public override string BaseUrl { get; set; } = "https://accounts.google.com/o/oauth2/auth";
		public override Uri RedirectUrl { get;set; } =  new Uri("http://localhost");
	
		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["scope"] = string.Join(" ", Scope);
			data ["redirect_uri"] = RedirectUrl.AbsoluteUri;
			return data;
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
