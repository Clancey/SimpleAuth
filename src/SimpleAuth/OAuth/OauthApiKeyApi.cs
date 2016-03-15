using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
    public class OauthApiKeyApi : OAuthApi
    {
		public OauthApiKeyApi(string identifier,string apiKey,string authKey, AuthLocation authLocation, string clientId, string clientSecret, string tokenUrl, string authorizationUrl, string redirectUrl = "http://localhost", HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{
			this.TokenUrl = tokenUrl;
			authenticator = new OAuthAuthenticator(authorizationUrl, tokenUrl, redirectUrl, clientId, clientSecret);

			AuthLocation = authLocation;
			AuthKey = authKey;
			ApiKey = apiKey;
		}
		public OauthApiKeyApi(string identifier, string apiKey, string authKey, AuthLocation authLocation , OAuthAuthenticator authenticator, HttpMessageHandler handler = null) : base(identifier, authenticator, handler)
	    {

			AuthLocation = authLocation;
			AuthKey = authKey;
		    ApiKey = apiKey;
	    }

		public string ApiKey { get; protected set; }
		public AuthLocation AuthLocation { get; protected set; }
		public string AuthKey { get; protected set; }

		protected override Task<string> PrepareUrl(string path, bool authenticated = true)
		{
			if (AuthLocation != AuthLocation.Query)
				return base.PrepareUrl(path, authenticated);
			return ApiKeyApi.PrepareUrl(BaseAddress, path, ApiKey, AuthKey, AuthLocation);

		}
		public override async Task PrepareClient(HttpClient client)
		{
			await base.PrepareClient(client);
			if (AuthLocation == AuthLocation.Header)
				client.DefaultRequestHeaders.Add(AuthKey, Identifier);
		}
	}
}
