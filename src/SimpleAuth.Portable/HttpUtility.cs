using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace System.Web
{
	internal sealed class HttpUtility
	{
		public static NameValueCollection ParseQueryString(string query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if ((query.Length > 0) && (query[0] == '?'))
			{
				query = query.Substring(1);
			}

			return new NameValueCollection(query, true);
		}

		public static string UrlEncode(string arg)
		{
			return WebUtility.UrlEncode(arg);
		}
	}

	public sealed class HttpValue
	{
		public HttpValue()
		{
		}

		public HttpValue(string key, string value)
		{
			Key = key;
			Value = value;
		}

		public string Key { get; set; }
		public string Value { get; set; }
	}
}