using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace SimpleAuth
{

    public class Account
	{
		public Account()
		{
		}

        public string Identifier { get; set; }

        public Dictionary<string, string> UserData { get; set; } = new Dictionary<string, string>();

		public virtual bool IsValid()
		{
			return true;
		}

		public virtual void Invalidate ()
		{

		}
	}
}
