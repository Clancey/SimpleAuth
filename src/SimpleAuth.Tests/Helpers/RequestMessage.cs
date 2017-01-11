using System;
using System.Net;
using System.Net.Http;
namespace SimpleAuth.Tests
{
	public struct RequestMessage
	{
		public string Url { get; set; }
		public HttpMethod Method { get; set; }
		public string Content { get; set; }

		public override bool Equals (object obj)
		{
			return obj?.ToString() == this.ToString();
		}
		public override int GetHashCode ()
		{
			return this.ToString ().GetHashCode ();
		}

		public override string ToString ()
		{
			return $"{{\"Url\":\"{Url}\",\"Method\":{{\"Method\":\"{Method}\"}},\"Content\":\"{Content}\"}}";
		}
	}

	public class RequestResponse
	{
		public string Content { get; set; }
		public string ContentType { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string RequiredAuthToken { get; set; }
	}
}
