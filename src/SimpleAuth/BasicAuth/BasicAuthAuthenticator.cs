using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
    public class BasicAuthAuthenticator : Authenticator
    {
	    readonly HttpClient client;
	    readonly string loginUrl;

	    public BasicAuthAuthenticator(HttpClient client, string loginUrl)
	    {
		    this.client = client;
		    this.loginUrl = loginUrl;
	    }

	    public virtual async Task<bool> CheckCredentails(string username, string password)
	    {
		    if(string.IsNullOrWhiteSpace(username))
				throw new Exception("Invalid Username");
			if (string.IsNullOrWhiteSpace(password))
				throw new Exception("Invalid Password");

			var key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

		    var response = await client.GetAsync(loginUrl);
		    response.EnsureSuccessStatusCode();
			FoundAuthCode(key);
			return true;
	    }
		
    }
}
