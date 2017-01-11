using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace SimpleAuth.Tests
{
	public class FakeHttpHandler : HttpMessageHandler
	{
		Dictionary<RequestMessage, RequestResponse> urlResponses;
		public FakeHttpHandler (Dictionary<RequestMessage, RequestResponse> urlResponses)
		{
			this.urlResponses = urlResponses;
		}

		protected override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			//Always default to some content, even if its an empty string
			var responseMessage = new HttpResponseMessage (HttpStatusCode.NotFound) {
				Content = new StringContent (""),
			};

			//Yes this is ugly, but it's to fix this awesomeness: https://github.com/dotnet/roslyn/issues/9513
			var content = await (request?.Content?.ReadAsStringAsync () ?? Task.FromResult(""));
			var rm = new RequestMessage {
				Content = content,
				Method = request.Method,
				Url = request.RequestUri.AbsoluteUri,
			};
			var json = rm.ToJson ();
			RequestResponse resp;
			if (urlResponses.TryGetValue (rm, out resp)) {

				//Check Auth token
				if (!string.IsNullOrWhiteSpace (resp.RequiredAuthToken)) {
					var headerValue = request?.Headers?.Authorization?.Parameter;
					if(resp.RequiredAuthToken != headerValue)
					{
						responseMessage.StatusCode = HttpStatusCode.Unauthorized;
						return responseMessage;
					}
				}
				responseMessage.StatusCode = resp.StatusCode;
				if (!string.IsNullOrEmpty (resp.Content)) {
					responseMessage.Content = new StringContent (resp.Content ?? "", System.Text.Encoding.UTF8, resp.ContentType);
				}
			} else {
				Console.WriteLine (rm.ToJson ());
			}
		
			return responseMessage;
		}
	}


}
