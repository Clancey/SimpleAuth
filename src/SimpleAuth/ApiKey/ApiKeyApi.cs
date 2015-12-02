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
			CurrentAccount = new Account{ Identifier = apiKey };
		}

		public AuthLocation AuthLocation { get; protected set; }
		public string AuthKey { get; protected set; }

		public override async Task<Account> Authenticate()
		{
			return new Account {Identifier = Identifier};
		}

		protected override async Task<Account> PerformAuthenticate(string[] scope)
		{
			return new Account {Identifier = Identifier};
		}

		protected override async Task<bool> RefreshAccount(Account account)
		{
			return true;
		}

		protected override async Task<string> PrepareUrl(string path, bool authenticated = true)
		{
			if (AuthLocation != AuthLocation.Query || !authenticated)
				return await base.PrepareUrl(path, authenticated);

			var url = BaseAddress != null ? new Uri (BaseAddress, path.TrimStart ('/')) : new Uri(path);

			var query = url.Query;
			var simplePath = string.IsNullOrWhiteSpace(query) ? path : path.Replace(query,"");

			var parameters = HttpUtility.ParseQueryString (query);
			parameters[AuthKey] = Identifier;
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