using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuth
{
    public class OAuthAccount : Account
    {
		string tokenType;
		public string TokenType
		{
			get
			{
				return tokenType;
			}
			set
			{
				if (value == "bearer")
					value = "Bearer";
				tokenType = value;
			}
		}

		public string Token { get; set; }

		public string RefreshToken { get; set; }

		public long ExpiresIn { get; set; }
		//UTC Datetime created
		public DateTime Created { get; set; }

		public string[] Scope { get; set; } = new string [0];

		public string ClientId { get; set; }

	    public override bool IsValid()
	    {
			if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(RefreshToken))
				return false;
			var expireTime = Created.AddSeconds(ExpiresIn);
			return expireTime > DateTime.UtcNow;
		}
    }
}
