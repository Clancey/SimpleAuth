//
//  Copyright 2018  (c) James Clancey
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
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Web;
namespace SimpleAuth.Providers {
	public class FitBitApi : SimpleAuth.OAuthApi {
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SimpleAuth.Providers.FitBitApi"/> class.
		/// </summary>
		/// <param name="serviceId">Service identifier.</param>
		/// <param name="clientId">Client identifier.</param>
		/// <param name="secret">If Implicit, Encryption key for storing data. If not, this is the client secret</param>
		/// <param name="isImplicit">If set to is implicit auth, no client secret required.</param>
		/// <param name="redirectUrl">Redirect URL.</param>
		public FitBitApi (string serviceId, string clientId, string secret, bool isImplicit, string redirectUrl = "http://localhost") : base (serviceId, clientId, secret)
		{
			BaseAddress = new Uri ("https://api.fitbit.com/1/");
			RedirecUri = new Uri (redirectUrl);
			Implicit = isImplicit;
			this.TokenUrl = "https://api.fitbit.com/oauth2/token";
		}
		bool Implicit;

		public
		Uri RedirecUri;
		protected override WebAuthenticator CreateAuthenticator ()
		{
			return new FitbitAuthenticator {
				ClientId = ClientId,
				ClientSecret = ClientSecret,
				Scope = Scopes.ToList (),
				RedirectUrl = RedirecUri,
				IsImplicit = Implicit,
				Title = "Login to Fitbit",
				BaseUrl = "https://www.fitbit.com/oauth2/authorize",
			};
		}

		protected override async Task<OAuthAccount> GetAccountFromAuthCode (WebAuthenticator authenticator, string identifier)
		{
			if (Implicit) {
				return new OAuthAccount () {
					ExpiresIn = 31536000,
					Created = DateTime.UtcNow,
					RefreshToken = authenticator.AuthCode,
					Scope = authenticator.Scope?.ToArray (),
					TokenType = "Bearer",
					Token = authenticator.AuthCode,
					ClientId = ClientId,
					Identifier = identifier,
					Cookies = authenticator.Cookies,
				};
			}
			//Fitbit does a weird AuthHeader before swaping tokens
			var authHeader = Convert.ToBase64String (System.Text.Encoding.UTF8.GetBytes ($"{ClientId}:{ClientSecret}"));
			Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue ("Basic", authHeader);
			var account = await base.GetAccountFromAuthCode (authenticator, identifier);
			return account;
		}

	}

	public class FitbitAuthenticator : OAuthAuthenticator {
		public bool IsImplicit { get; set; }
		public override bool CheckUrl (Uri url, Cookie [] cookies)
		{
			try {
				Console.WriteLine (url);
				//For some reason FitBit doesnt do proper urls for implicit :(
				if (url?.AbsoluteUri.Contains ("#access_token") ?? false)
					url = new Uri (url.AbsoluteUri.Replace ("#access_token", "?access_token"));
				if (url == null || string.IsNullOrWhiteSpace (url.Query))
					return false;
				if (url.Host != RedirectUrl.Host)
					return false;
				var parts = HttpUtility.ParseQueryString (url.Query);
				var codeKey = IsImplicit ? "access_token" : "code";
				var code = parts [codeKey];
				if (!string.IsNullOrWhiteSpace (code)) {
					Cookies = cookies?.Select (x => new CookieHolder { Domain = x.Domain, Path = x.Path, Name = x.Name, Value = x.Value }).ToArray ();
					FoundAuthCode (code);
					return true;
				}
				var success = base.CheckUrl (url, cookies);
				if (success)
					Console.WriteLine ("Success");

				return success;
			} catch {
				return false;
			}
		}

		public override Dictionary<string, string> GetInitialUrlQueryParameters ()
		{
			var parameters = base.GetInitialUrlQueryParameters ();
			parameters ["response_type"] = IsImplicit ? "token" : "code";
			if (IsImplicit) {
				//Let the token last for a year, so you don't show the login UI everyday.
				parameters ["expires_in"] = "31536000";
			}
			return parameters;
		}

	}
}
