using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth
{
    public abstract class WebAuthenticator : Authenticator
	{
		public bool ClearCookiesBeforeLogin { get; set; }

		public abstract string BaseUrl { get;set;}

		public abstract Uri RedirectUrl{get;set;}

		public List<string> Scope { get; set; } = new List<string>();

		public virtual bool CheckUrl(Uri url, Cookie[] cookies)
		{
			try
			{
				if (url == null || string.IsNullOrWhiteSpace(url.Query))
					return false;
				if (url.Host != RedirectUrl.Host)
					return false;
				var parts = HttpUtility.ParseQueryString(url.Query);
				var code = parts["code"];
				if (!string.IsNullOrWhiteSpace(code) && tokenTask != null)
				{
					FoundAuthCode(code);
					return true;
				}

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return false;
		}



#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public virtual async Task<Uri> GetInitialUrl()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run s
		{
			var uri = new Uri(BaseUrl);
			var collection = GetInitialUrlQueryParameters();
			return uri.AddParameters(collection);
		}

	    public virtual NameValueCollection GetInitialUrlQueryParameters()
	    {

			var scope = string.Join(" ", Scope);
			return new NameValueCollection()
			{
				{"client_id",ClientId},
				{"scope",scope},
				{"response_type","code"},
				{"redirect_uri",RedirectUrl.AbsoluteUri}
			};

	    }

#pragma warning disable 1998
		public virtual async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
#pragma warning restore 1998
		{
			return new Dictionary<string, string> {
				{
					"grant_type",
					"authorization_code"
				}, {
					"code",
					AuthCode
				}, {
					"client_id",
					ClientId
				}, {
					"client_secret",
					clientSecret
				},
			};
		}


	}
}
