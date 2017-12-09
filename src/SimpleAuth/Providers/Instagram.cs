using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class InstagramApi : OAuthApi
	{
		public InstagramApi(string identifier, string clientId, string clientSecret, string redirectUrl = "http://localhost", HttpMessageHandler handler = null)
			: base(identifier, clientId, clientSecret, handler)
		{
			this.TokenUrl = "https://api.instagram.com/oauth/access_token";
			this.RedirectUrl = new Uri(redirectUrl);
			Scopes = new[] { "basic" };
		}

		public Uri RedirectUrl { get; private set; }

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new InstagramAuthenticator
			{
				Scope = Scopes.ToList(),
				ClientId = ClientId,
				ClearCookiesBeforeLogin = CalledReset,
				RedirectUrl = RedirectUrl,
			};
		}
	}

	public class InstagramAuthenticator : OAuthAuthenticator
	{
		public override string BaseUrl { get; set; } = "https://api.instagram.com/oauth/authorize";

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["redirect_uri"] = RedirectUrl.OriginalString;
			return data;
		}
	}
}

