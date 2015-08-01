using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
			return new GoogleMusicAuthenticator
			{
				Scope = scope.ToList(),
				ClientId = ClientId,
			};
		}
	}

	class GoogleMusicAuthenticator : Authenticator
	{
		public override string BaseUrl { get; set; } = "https://accounts.google.com/o/oauth2/auth";
		public override Uri RedirectUrl { get;set; } =  new Uri("http://localhost");
	

		public override void CheckUrl(Uri url, Cookie[] cookies)
		{
			if (cookies.Length == 0)
				return;
			var cookie = cookies.FirstOrDefault(x => x.Name.IndexOf("oauth_code", StringComparison.InvariantCultureIgnoreCase) == 0);
			if (!string.IsNullOrWhiteSpace(cookie?.Value))
				FoundAuthCode(cookie.Value);
		}

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["scope"] = string.Join(" ", Scope);
			return data;
		}
	}

}
