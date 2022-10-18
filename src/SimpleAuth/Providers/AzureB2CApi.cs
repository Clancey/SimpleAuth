using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class AzureB2CApi : OAuthApi
    {
		private string AuthorizeUrl { get; set; }
		private string RedirectUrl { get; set; }
		bool UsesClientSecret;

        /// <summary>
        /// Create an AzureB2C API.
        ///
        /// Use this contructor if you use Azure Front Door as a proxy to your B2C instance, and provide
        /// the domain name that you proxy B2C calls through.
        /// </summary>
        /// <param name="identifier">The identifier for this API</param>
        /// <param name="clientId">The client ID value</param>
        /// <param name="proxyAddress">The domain name that you use with Azure Front Door</param>
        /// <param name="tenant">The tenant ID</param>
        /// <param name="policy">The B2C policy to use for the the authentication flow</param>
        /// <param name="redirectUrl">The redirect URL to use</param>
        /// <param name="clientSecret">(optional) The client secret.</param>
        /// <param name="handler">(optional) The HTTP client handler</param>
        public AzureB2CApi(
            string identifier,
            string clientId,
            string proxyAddress,
            string tenant,
            string policy,
            string redirectUrl,
            string clientSecret = "",
            HttpMessageHandler handler = null)
                : base(identifier, clientId, string.Empty, handler)
        {
            UsesClientSecret = false;
            this.AuthorizeUrl = $"https://{proxyAddress}/{tenant}.onmicrosoft.com/{policy}/oauth2/v2.0/authorize";
            this.TokenUrl = $"https://{proxyAddress}/{tenant}.onmicrosoft.com/{policy}/oauth2/v2.0/token";
            this.RedirectUrl = redirectUrl;
            authenticator = new AzureB2CAuthenticator(AuthorizeUrl, TokenUrl, RedirectUrl, clientId, clientSecret);
        }

        /// <summary>
        /// Create an AzureB2C API.
        ///
        /// Use this contructor if you are using the default B2C URLs to authenticate your application.
        /// </summary>
        /// <param name="identifier">The identifier for this API</param>
        /// <param name="clientId">The client ID value</param>
        /// <param name="tenant">The tenant ID</param>
        /// <param name="policy">The B2C policy to use for the the authentication flow</param>
        /// <param name="redirectUrl">The redirect URL to use</param>
        /// <param name="clientSecret">(optional) The client secret.</param>
        /// <param name="handler">(optional) The HTTP client handler</param>
        public AzureB2CApi(
			string identifier,
			string clientId,
			string tenant,
			string policy,
			string redirectUrl,
            string clientSecret = "",
			HttpMessageHandler handler = null)
				: base (identifier, clientId, clientSecret, handler)
		{
			UsesClientSecret = false;
			this.AuthorizeUrl = $"https://{tenant}.b2clogin.com/{tenant}.onmicrosoft.com/{policy}/oauth2/v2.0/authorize";
            this.TokenUrl = $"https://{tenant}.b2clogin.com/{tenant}.onmicrosoft.com/{policy}/oauth2/v2.0/token";
            this.RedirectUrl = redirectUrl;
            authenticator = new AzureB2CAuthenticator(AuthorizeUrl, TokenUrl, RedirectUrl, clientId, string.Empty);
        }
    }

	public class AzureB2CAuthenticator : OAuthAuthenticator
	{
        public AzureB2CAuthenticator(
			string authUrl,
			string tokenUrl,
			string redirectUrl,
			string clientId,
			string clientSecret)
				: base(authUrl, tokenUrl, redirectUrl, clientId, clientSecret)
        {

        }

        public override Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
        {
            var tokenPostData = new Dictionary<string, string> {
                {
                    "grant_type",
                    "authorization_code"
                },
                {
                    "code",
                    AuthCode
                },
                {
                    "client_id",
                    ClientId
                },
                {
                    "client_secret",
                    clientSecret
                }
            };

            // AzureB2C requires the scope to be include, which is not done in the base
            // OAuthAuthenticator implemntation.
            if (Scope?.Count > 0)
            {
                var scope = string.Join(" ", Scope);
                tokenPostData["scope"] = scope;
            }

            if (RedirectUrl != null)
            {
                tokenPostData["redirect_uri"] = RedirectUrl.OriginalString;
            }

            return Task.FromResult(tokenPostData);
        }
    }
}

