using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

namespace SimpleAuth.Providers
{
	public class MicrosoftLiveConnectApi : OAuthApi
	{
		public string RedirectUrl { get; set; }

		public MicrosoftLiveConnectApi(string identifier, string clientId, string clientSecret, string redirectUrl = "http://localhost/", HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{
			RedirectUrl = redirectUrl;
			TokenUrl = "https://login.live.com/oauth20_token.srf";
		}

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new MicrosoftLiveConnectAuthenticator(ClientId, ClientSecret, RedirectUrl)
			{
				Scope = Scopes.ToList(),

			};
		}
	}
	class MicrosoftLiveConnectAuthenticator : OAuthAuthenticator
	{
		public MicrosoftLiveConnectAuthenticator(string clientId, string clientSecret, string redirectUrl = "http://localhost/") : base("https://login.live.com/oauth20_authorize.srf", "https://login.live.com/oauth20_token.srf", redirectUrl, clientId, clientSecret)
		{

		}

		public override Dictionary<string, string> GetInitialUrlQueryParameters()
		{
			var parameters = base.GetInitialUrlQueryParameters();
#if !__PCL__
			parameters["locale"] = Thread.CurrentThread.CurrentCulture?.TwoLetterISOLanguageName;
#endif
			parameters["display"] = "touch";
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
