using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SimpleAuth
{
    [DataContract]
    public class OAuthAccount : Account
    {

		string tokenType;
        [DataMember]
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
        [DataMember]
        public string IdToken { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string RefreshToken { get; set; }
        [DataMember]
        public long ExpiresIn { get; set; }
        //UTC Datetime created
        [DataMember]
        public DateTime Created { get; set; }
        [DataMember]
        public string[] Scope { get; set; } = new string [0];
        [DataMember]
        public string ClientId { get; set; }
        [DataMember]
        public CookieHolder [] Cookies { get; set; }

	    public override bool IsValid()
	    {
			if (string.IsNullOrWhiteSpace(Token))
				return false;
			// This allows you to specify -1 for never expires
		    if (ExpiresIn <= 0)
			    return true;
			if(string.IsNullOrWhiteSpace(RefreshToken))
				return false;
			var expireTime = Created.AddSeconds(ExpiresIn);
			return expireTime > DateTime.UtcNow;
		}

		public override void Invalidate ()
		{
			base.Invalidate ();
			ExpiresIn = 1;
			Token = null;
		}
    }
}
