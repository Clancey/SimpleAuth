using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuth
{
    public class OAuthAuthenticator : WebAuthenticator
	{
	    public string ClientSecret { get; set; }

		protected OAuthAuthenticator()
		{

		}

	    public OAuthAuthenticator(string authUrl, string tokenUrl,string redirectUrl, string clientId, string clientSecret)
	    {
		    this.ClientId = clientId;
		    this.ClientSecret = clientSecret;
		    this.TokenUrl = tokenUrl;
		    BaseUrl = authUrl;
		    RedirectUrl = new Uri(redirectUrl);
	    }

		public string Identifier { get; set; }

		public string TokenUrl { get; set; }
		public override string BaseUrl { get; set;}
		public override Uri RedirectUrl { get; set;}
    }
}
