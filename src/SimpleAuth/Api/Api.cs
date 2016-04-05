using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth
{
	public class Api
	{
		public bool Verbose { get; set; } = false;
		public string Identifier {get; private set;}

		public string SharedGroupAccess { get; set; }

		public virtual string ExtraDataString { get; set; }

		public bool EnsureApiStatusCode { get; set; } = true;

		public string DefaultMediaType { get; set; } = "application/json";

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

		public string DefaultAccepts { get; set; }

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


		public string DeviceId { get; set; }

		protected virtual Task OnAccountUpdated(Account account)
		{
			return Task.FromResult(true);
		}
		public virtual IAuthStorage AuthStorage {
			get {
				return Resolver.GetObject<IAuthStorage> ();
			}
		}
		protected bool CalledReset = false;
		public virtual void ResetData()
		{
			CalledReset = true;
			AuthStorage.SetSecured(Identifier,"",ClientId,ClientSecret,SharedGroupAccess);
		}

		public virtual async Task PrepareClient(HttpClient client)
		{
			await VerifyCredentials();
			if (!string.IsNullOrWhiteSpace (UserAgent))
				client.DefaultRequestHeaders.Add ("User-Agent", UserAgent);
		}

		public async virtual Task<Stream> GetUrlStream(string path, bool authenticated = true)
		{
			if (authenticated)
				await VerifyCredentials();
			path = await PrepareUrl(path);
			return await Client.GetStreamAsync(new Uri(path));
		}

		public virtual Task<string> Get(string path = null, Dictionary<string, string> queryParameters = null,
			Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{

			return SendObjectMessage(path, null, HttpMethod.Get, queryParameters, headers, authenticated, methodName);
		}
        public virtual async Task<T> Get<T>(string path = null, Dictionary<string, string> queryParameters = null,
			Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
        {
			var data = await Get(path, queryParameters, headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data);
		}

		
		public virtual async Task<T> Post<T>(object body,string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var data = await Post(body,path, queryParameters, headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data,body);
		}
		public virtual Task<string> Post(object body, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return SendObjectMessage(path,body, HttpMethod.Post, queryParameters, headers, authenticated, methodName);
		}

		public virtual async Task<T> Post<T>(HttpContent content, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var data = await Post(content,path,  queryParameters,  headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data);
		}

		public virtual Task<string> Post(HttpContent content, string path = null, Dictionary<string, string> queryParameters = null,
			Dictionary<string, string> headers = null, bool authenticated = true,
			[System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return SendObjectMessage(path,content, HttpMethod.Post, queryParameters, headers, authenticated, methodName);
		}


		public virtual async Task<T> Put<T>(HttpContent content, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var data = await Put(content,path, queryParameters, headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data);
		}

		public virtual Task<string> Put (HttpContent content, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return SendObjectMessage(path,content, HttpMethod.Put, queryParameters, headers, authenticated, methodName);
		}

		public virtual async Task<T> Put<T>(object body, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var data = await Put(body,path, queryParameters, headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data, body);
		}


		public virtual Task<string> Put(object body, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return SendObjectMessage(path, body, HttpMethod.Put, queryParameters, headers, authenticated, methodName);
		}

		public virtual async Task<T> Delete<T>(object body, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var data = await Delete(body,path, queryParameters, headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data, body);
		}

		public virtual async Task<T> Delete<T>(HttpContent content = null, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var data = await Delete(content,path, queryParameters, headers, authenticated, methodName).ConfigureAwait(false);
			return Deserialize<T>(data);
		}

		public virtual Task<string> Delete(HttpContent content = null, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return SendObjectMessage(path,content, HttpMethod.Delete, queryParameters, headers, authenticated, methodName);
		}

		public virtual Task<string> Delete(object body, string path = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			return SendObjectMessage(path,body, HttpMethod.Delete, queryParameters, headers, authenticated, methodName);
		}

		public virtual async Task<string> PostUrl(string path, string content, string mediaType = "", bool authenticated = true)
		{
			var dataType = string.IsNullOrWhiteSpace (mediaType) ? DefaultMediaType : mediaType;
			var message = await PostMessage(path,new StringContent(content, System.Text.Encoding.UTF8, dataType),authenticated);
			return await message.Content.ReadAsStringAsync();
		}

		public virtual async Task<T> Post<T>(string path, string content, bool authenticated = true)
		{
			Debug.WriteLine("{0} - {1}", path, content);
			var data = await PostUrl(path, content, authenticated: authenticated).ConfigureAwait(false);
			if(Verbose)
				Debug.WriteLine(data);
			return Deserialize<T>(data);

		}
		public virtual async Task<T> Post<T>(string path, HttpContent content, bool authenticated = true)
		{
			if(Verbose)
				Debug.WriteLine("{0} - {1}", path, await content.ReadAsStringAsync());
			var resp = await PostMessage(path, content,authenticated).ConfigureAwait(false);
			var data = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
			if(Verbose)
				Debug.WriteLine(data);
			return Deserialize<T>(data);

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
		public virtual Task<string> SendObjectMessage(string path, object body, HttpMethod method, Dictionary<string, string> queryParameters, Dictionary<string, string> headers, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			var mediaType = DefaultMediaType;
			headers?.TryGetValue("Content-Type", out mediaType);

			var bodyJson = body.ToJson();
			var content = new StringContent(bodyJson, System.Text.Encoding.UTF8, mediaType);

			return SendObjectMessage(path, content, method, queryParameters, headers, authenticated, methodName);
		}

		public virtual async Task<string> SendObjectMessage(string path, HttpContent content, HttpMethod method, Dictionary<string, string> queryParameters, Dictionary<string, string> headers, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				path = GetType().GetMethods().Where(x=> x.Name == methodName).Select(x => GetValueFromAttribute<PathAttribute>(x)).Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault();
			}

			if (string.IsNullOrWhiteSpace(path))
				throw new Exception("Missing Path Attribute");

			if(queryParameters != null)
				path = CombineUrl(path, queryParameters);

			//Merge attributes with passed in headers.
			//Passed in headers overwrite attributes
			var attributeHeaders = GetType().GetMethods().Where(x=> x.Name == methodName).Select(x => GetHeadersFromMethod(x)).Where(x => x?.Any() ?? false).FirstOrDefault();
			if (attributeHeaders?.Any() ?? false)
			{
				if(headers != null)
					foreach (var header in headers)
					{
						attributeHeaders[header.Key] = header.Value;
					}
				headers = attributeHeaders;
			}
            

			var message = await SendMessage(path, content, method, headers, authenticated);
			if(EnsureApiStatusCode)
				message.EnsureSuccessStatusCode();
			var data = await message.Content.ReadAsStringAsync();
			return data;
		}


		public async Task<HttpResponseMessage> SendMessage(string path, HttpContent content, HttpMethod method , Dictionary<string, string> headers = null, bool authenticated = true,HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
		{
			if (authenticated)
				await VerifyCredentials();
			path = await PrepareUrl(path);
			var uri = BaseAddress != null ? new Uri(BaseAddress, path.TrimStart('/')) : new Uri(path);
			var request = new HttpRequestMessage
			{
				Method = method,
				RequestUri = uri,
				Content =  content,
			};

			MergeHeaders(request.Headers, headers);

			return await SendMessage(request, authenticated, completionOption);
		}

		public async Task<HttpResponseMessage> SendMessage(HttpRequestMessage message, bool authenticated = true,HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
		{
			if (authenticated)
				await VerifyCredentials();
			return await Client.SendAsync(message,completionOption);
		}

		protected virtual Task VerifyCredentials()
		{
			return Task.FromResult(true);
		}

		protected virtual  Task<string> PrepareUrl(string path, bool authenticated = true)
		{

			return Task.FromResult(path);
		}

		protected virtual T Deserialize<T>(string data)
		{
			if (typeof(T) == data.GetType())
				return (T)(object)data;
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
		protected virtual T Deserialize<T>(string data, object inObject)
		{
			try
			{
				if (inObject is T)
				{
					var serializer = new Newtonsoft.Json.JsonSerializer();
					using (var reader = new StringReader(data))
					{
						var outObj = (T)inObject;
						serializer.Populate(reader,outObj);
						return outObj;
					}
				}
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
			if (method == null)
				return headers;
			var accepts = GetValueFromAttribute<AcceptsAttribute>(method);
			if (!string.IsNullOrWhiteSpace(accepts))
				headers["Accept"] = accepts;
			else if (!string.IsNullOrWhiteSpace(DefaultAccepts))
				headers["Accept"] = DefaultAccepts;
			var contentType = GetValueFromAttribute<ContentTypeAttribute>(method);
			if (!string.IsNullOrWhiteSpace(contentType))
				headers["Content-Type"] = contentType;
			return headers;
		}

		public string GetValueFromAttribute<T>(MethodInfo method) where T : StringValueAttribute
		{
			return method?.GetCustomAttributes(true).OfType<T>().FirstOrDefault()?.Value;
		}

		public static void MergeHeaders(HttpRequestHeaders inHeaders, Dictionary<string, string> newHeaders)
		{
			if (newHeaders == null)
				return;
			foreach (var header in newHeaders)
			{
				if (header.Key == "Content-Type")
					continue;
				inHeaders.Add(header.Key,header.Value);
			}
		}

		public async Task<bool> Ping(string url)
		{
			try{
				var request = new HttpRequestMessage(HttpMethod.Get, url);
				await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
				return true;
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return false;
		}

		public Task<bool> Ping()
		{
			return Ping(BaseAddress.AbsoluteUri);
		}
	}
}

