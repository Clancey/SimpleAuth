using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth
{
	public abstract class Api
	{
		public bool Verbose { get; set; } = false;
		public string Identifier {get; private set;}

		public string SharedGroupAccess { get; set; }

		public virtual string ExtraDataString { get; set; }

		protected string ClientSecret;

		protected string ClientId;
		
		protected HttpClient Client;

		public readonly HttpMessageHandler Handler;

		string userAgent;
		public string UserAgent {
			get {
				return userAgent;
			}
			set {
				userAgent = value;
				Client.DefaultRequestHeaders.Add ("User-Agent", UserAgent);
			}
		}

		public Api(string identifier, HttpMessageHandler handler = null)
		{
			Identifier = identifier;
			Handler = handler;
			Client = handler == null ? new HttpClient() : new HttpClient(handler);
		}

		public Uri BaseAddress {
			get { return Client.BaseAddress; }
			set { Client.BaseAddress = value; }
		}

		public bool HasAuthenticated { get; private set; }



		protected Account currentAccount;

		public Account CurrentAccount
		{
			get
			{
				return currentAccount;
			}
			protected set
			{
				currentAccount = value;
				HasAuthenticated = true;
#pragma warning disable 4014
				PrepareClient(Client);
				OnAccountUpdated(currentAccount);
#pragma warning restore 4014
			}
		}

		readonly object authenticationLocker = new object();
		Task<Account> authenticateTask;
		public async Task<Account> Authenticate()
		{
			lock (authenticationLocker)
			{
				if (authenticateTask == null || authenticateTask.IsCompleted)
				{
					authenticateTask = PerformAuthenticate();
				}
			}
			return await authenticateTask;
		}
		
		protected abstract Task<Account> PerformAuthenticate();

		protected abstract Task<bool> RefreshAccount(Account account);

		public string DeviceId { get; set; }

		protected virtual async Task OnAccountUpdated(Account account)
		{

		}
		public virtual IAuthStorage AuthStorage {
			get {
				return Resolver.GetObject<IAuthStorage> ();
			}
		}
		protected bool CalledReset = false;
		public virtual void ResetData()
		{
			CurrentAccount = null;
			CalledReset = true;
			AuthStorage.SetSecured(Identifier,"",ClientId,ClientSecret,SharedGroupAccess);
		}

		protected virtual void SaveAccount(Account account)
		{
			AuthStorage.SetSecured(account.Identifier, SerializeObject(account),ClientId, ClientSecret,SharedGroupAccess);
		}

		protected virtual T GetAccount<T>(string identifier) where T : Account
		{
			try
			{
				var data = AuthStorage.GetSecured(identifier, ClientId, ClientSecret,SharedGroupAccess);
				return string.IsNullOrWhiteSpace(data) ? null : Deserialize<T>(data);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return null;
			}
		}
		
		public virtual async Task PrepareClient(HttpClient client)
		{
			await VerifyCredentials();
			if (!string.IsNullOrWhiteSpace (UserAgent))
				client.DefaultRequestHeaders.Add ("User-Agent", UserAgent);
		}

		public async virtual Task<List<T>> GetGenericList<T>(string path, bool authenticated = true)
		{
			var items = await Get<List<T>>(path,authenticated: authenticated);

			return items;
		}

		public async virtual Task<Stream> GetUrlStream(string path, bool authenticated = true)
		{
			if (authenticated)
				await VerifyCredentials();
			path = await PrepareUrl(path);
			return await Client.GetStreamAsync(new Uri(path));
		}

		public virtual Task<T> Get<T>(bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return Get<T>(null, authenticated, methodName);
		}

		public virtual Task<T> Get<T>(Dictionary<string, string> queryParameters, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var headers = GetHeadersFromMethod(GetType().GetMethod(methodName));
            return Get<T>(queryParameters, headers, authenticated, methodName);
		}
		public virtual Task<T> Get<T>(Dictionary<string, string> queryParameters, Dictionary<string, string> headers, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var path = GetValueFromAttribute<PathAttribute>(GetType().GetMethod(methodName));
			if (string.IsNullOrWhiteSpace(path))
				throw new Exception("Missing Path Attribute");

			path = CombineUrl(path, queryParameters);
			return Get<T>(path, "",authenticated);
		}

		public virtual Task<T> Get<T>(string path, bool authenticated = true)
		{
			return Get<T>(path, null, null, authenticated);
		}

		public virtual async Task<T> Get<T>(string path, Dictionary<string, string> queryParameters,
			Dictionary<string, string> headers, bool authenticated = true)
		{
			var data = await GetString(path, queryParameters, headers, authenticated);
			return await Task.Run(() => Deserialize<T>(data));
		}
		public virtual Task<T> Get<T>(string path, string id, bool authenticated = true)
		{
			return Get<T>(Path.Combine(path, id), null, null, authenticated);
		}


		public virtual Task<string> GetString(Dictionary<string,string> queryParameters, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var path = GetType().GetMethod(methodName).GetCustomAttributes(true).OfType<PathAttribute>().FirstOrDefault()?.Value;
			if(string.IsNullOrWhiteSpace(path))
				throw new Exception("Missing Path Attribute");

			path = CombineUrl(path, queryParameters);
			return GetString(path, authenticated);
		}

		/// <summary>
		/// Gets the string content from the url provided from  PathAttribute on the method name
		/// </summary>
		/// <param name="authenticated"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public virtual Task<string> GetString(bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var path = GetType().GetMethod(methodName).GetCustomAttributes(true).OfType<PathAttribute>().FirstOrDefault()?.Value;
			if (string.IsNullOrWhiteSpace(path))
				throw new Exception("Missing Path Attribute");
			return GetString(path, authenticated);
		}

		public async virtual Task<string> GetString(string path, bool authenticated = true)
		{
			var resp = await GetMessage(path,authenticated: authenticated);
			return await resp.Content.ReadAsStringAsync();
		}

		public async virtual Task<string> GetString(string path, Dictionary<string, string> queryParameters,
			Dictionary<string, string> headers, bool authenticated = true)
		{
			var resp = await GetMessage(path,headers, authenticated: authenticated);
			return await resp.Content.ReadAsStringAsync();
		}
		public virtual async Task<string> PostUrl(string path, string content, string mediaType = "text/json", bool authenticated = true)
		{
			var message = await PostMessage(path,new StringContent(content, System.Text.Encoding.UTF8, mediaType),authenticated);
			return await message.Content.ReadAsStringAsync();
		}

		public virtual async Task<T> Post<T>(string path, string content, bool authenticated = true)
		{
			Debug.WriteLine("{0} - {1}", path, content);
			var data = await PostUrl(path, content, authenticated: authenticated);
			if(Verbose)
				Debug.WriteLine(data);
			return await Task.Run(() => Deserialize<T>(data));

		}
		public virtual async Task<T> Post<T>(string path, HttpContent content, bool authenticated = true)
		{
			Debug.WriteLine("{0} - {1}", path, await content.ReadAsStringAsync());
			var resp = await PostMessage(path, content,authenticated);
			var data = await resp.Content.ReadAsStringAsync();
			Debug.WriteLine(data);
			return await Task.Run(() => Deserialize<T>(data));

		}
		public async Task<HttpResponseMessage> PostMessage(string path, HttpContent content, bool authenticated = true)
		{
			if(authenticated)
				await VerifyCredentials();
			path = await PrepareUrl(path);
			return await Client.PostAsync(path, content);
		}

		public async Task<HttpResponseMessage> PutMessage(string path, HttpContent content, bool authenticated = true)
		{
			if (authenticated)
				await VerifyCredentials();
			path = await PrepareUrl(path);
			return await Client.PutAsync(path, content);
		}

		public async Task<HttpResponseMessage> GetMessage(string path, Dictionary<string,string> headers = null,  bool authenticated = true)
		{
			path = await PrepareUrl(path);
			var uri = BaseAddress != null ? new Uri(BaseAddress, path.TrimStart('/')) : new Uri(path);
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = uri,
			};

			if (headers?.Any() == true)
			{
				foreach (var header in headers)
				{
					request.Headers.Add(header.Key, header.Value);
				}
			}

            return await SendMessage(request,authenticated);
		}

		public async Task<HttpResponseMessage> SendMessage(HttpRequestMessage message, bool authenticated = true)
		{
			if (authenticated)
				await VerifyCredentials();
			return await Client.SendAsync(message);
		}

		protected virtual async Task VerifyCredentials()
		{
			if (CurrentAccount == null)
			{
				throw new Exception("Not Authenticated");
			}
			if (!CurrentAccount.IsValid())
				await RefreshAccount(CurrentAccount);
		}

		protected virtual async Task<string> PrepareUrl(string path, bool authenticated = true)
		{
			return path;
		}

		protected virtual T Deserialize<T>(string data)
		{
			try
			{
				return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return default(T);
		}

		protected virtual string SerializeObject(object obj)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
		}

		public virtual string CombineUrl(string url, Dictionary<string, string> queryParameters)
		{
			if (queryParameters?.Any() != true)
				return url;

			var uri = BaseAddress != null ? new Uri(BaseAddress, url.TrimStart('/')) : new Uri(url);

			var query = uri.Query;
			var simplePath = string.IsNullOrWhiteSpace(query) ? url : url.Replace(query, "");

			var parameters = HttpUtility.ParseQueryString(query);

			foreach (var queryParameter in queryParameters)
			{
				var pathKey = "{" + queryParameter.Key + "}";
				if (simplePath.Contains(pathKey))
					simplePath = simplePath.Replace(pathKey, queryParameter.Value);
				else
					parameters[queryParameter.Key] = queryParameter.Value;
			}
			var newQuery = parameters.ToString();
			var newPath = $"{simplePath}?{newQuery}";
			return newPath;
		}

		public virtual Dictionary<string, string> GetHeadersFromMethod(MethodInfo method)
		{
			var headers = new Dictionary<string,string>();
			var accepts = GetValueFromAttribute<AcceptsAttribute>(method);
			if (!string.IsNullOrWhiteSpace(accepts))
				headers["Accept"] = accepts;
			var contentType = GetValueFromAttribute<ContentTypeAttribute>(method);
			if (!string.IsNullOrWhiteSpace(contentType))
				headers["Content-Type"] = contentType;
			return headers;
		}

		public string GetValueFromAttribute<T>(MethodInfo method) where T : StringValueAttribute
		{
			return method.GetCustomAttributes(true).OfType<T>().FirstOrDefault()?.Value;
		}
	}
}

