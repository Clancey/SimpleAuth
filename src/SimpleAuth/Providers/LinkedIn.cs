//
//  Copyright 2016  Clancey
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleAuth.Providers
{
	public class LinkedInApi : OAuthApi
	{
		public LinkedInApi(string identifier, string clientId, string clientSecret, string redirectUrl = "http://localhost", HttpMessageHandler handler = null) : base(identifier, new OAuthAuthenticator("https://www.linkedin.com/uas/oauth2/authorization", "https://www.linkedin.com/uas/oauth2/accessToken", redirectUrl, clientId, clientSecret), handler)
		{
			BaseAddress = new Uri("https://api.linkedin.com/v1/");
			Scopes = new string[] { "r_basicprofile" };
		}

		[Path("people")]
		public Task<string> GetProfile()
		{
			return Get();
		}
	}
}

