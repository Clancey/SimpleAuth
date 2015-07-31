using System;
using Newtonsoft.Json;

namespace SimpleAuth
{
	public class OauthResponse : ApiResponse
	{
		string tokenType;
		[JsonProperty ("token_type")]
		public string TokenType {
			get {
				return tokenType;
			}
			set {
				//Sanitize some inputs... Bearer should be upercased
				if (value == "bearer")
					value = "Bearer";
				tokenType = value;
			}
		}
		[JsonProperty("expires_in")]
		public long ExpiresIn {get;set;}

		[JsonProperty("refresh_token")]
		public string RefreshToken {get;set;}

		[JsonProperty("access_token")]
		public string AccessToken {get;set;}

		[JsonProperty("id_token")]
		public string Id { get; set; }
	}
}

