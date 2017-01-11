using System;
using SimpleAuth.Providers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleAuth.Tests
{
	public class OAuthTestApi : OAuthApi
	{
		public int CurrentShowAuthenticatorCallCount = 0;
		public OAuthTestApi (Dictionary<RequestMessage, RequestResponse> urlResponses) : base ("OAuthApiTest","clientId","clientSecret","https://localhost/o/oauth2/token","https://localhost/o/oauth2/token",handler: new FakeHttpHandler(urlResponses))
		{
			this.CurrentShowAuthenticator = (obj) => {
				CurrentShowAuthenticatorCallCount++;
				//Sendback a token!
				obj.CheckUrl ( new Uri($"https://localhost/?code=authtoken"), new System.Net.Cookie [0]);
			};
			ScopesRequired = false;
			BaseAddress = new Uri ("https://localhost");
		}

		public override System.Threading.Tasks.Task<bool> Ping (string url)
		{
			return Task.FromResult (true);
		}
	}
}
