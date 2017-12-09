using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class AmazonApi : OAuthApi
	{
		public AmazonApi(string identifier, string clientId, string clientSecret, string redirectUrl = "http://localhost", HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{
			TokenUrl = "https://api.amazon.com/auth/o2/token";
			RedirectUrl = new Uri(redirectUrl);
		}

		public Uri RedirectUrl { get; private set; }

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new AmazonAuthenticator
			{
				Scope = Scopes.ToList(),
				ClientId = ClientId,
				ClearCookiesBeforeLogin = CalledReset,
				RedirectUrl = RedirectUrl,
			};
		}
	}

	public class AmazonAuthenticator : WebAuthenticator
	{
		public override string BaseUrl { get; set; } = "https://www.amazon.com/ap/oa";
		public override Uri RedirectUrl { get; set; }
		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["redirect_uri"] = RedirectUrl.OriginalString;
			return data;
		}
	}
}
