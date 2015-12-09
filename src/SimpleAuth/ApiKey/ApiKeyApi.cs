using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth
{
	public class ApiKeyApi : Api
	{
		public ApiKeyApi(string apiKey, string authKey, AuthLocation authLocation, HttpMessageHandler handler = null)
			: base(apiKey, handler)
		{
			AuthLocation = authLocation;
			AuthKey = authKey;
		}

		public AuthLocation AuthLocation { get; protected set; }
		public string AuthKey { get; protected set; }

		protected override  Task<string> PrepareUrl(string path, bool authenticated = true)
		{

			if (AuthLocation != AuthLocation.Query || !authenticated)
				return base.PrepareUrl(path, authenticated);
			return PrepareUrl(BaseAddress,path,Identifier,AuthKey,AuthLocation);

		}
		public static async Task<string> PrepareUrl(Uri baseAddress, string path,string apiKey, string authKey, AuthLocation authLocation)
		{
			var url = baseAddress != null ? new Uri(baseAddress, path.TrimStart('/')) : new Uri(path);

			var query = url.Query;
			var simplePath = string.IsNullOrWhiteSpace(query) ? path : path.Replace(query, "");

			var parameters = HttpUtility.ParseQueryString(query);
			parameters[authKey] = apiKey;
			var newQuery = parameters.ToString();
			var newPath = $"{simplePath}?{newQuery}";
			return newPath;

		}
		public override async Task PrepareClient(HttpClient client)
		{
			await base.PrepareClient(client);
			if(AuthLocation == AuthLocation.Header)
				client.DefaultRequestHeaders.Add(AuthKey, Identifier);
		}
	}
}