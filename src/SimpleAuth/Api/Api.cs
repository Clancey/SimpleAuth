using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

		public Api(string identifier, HttpMessageHandler handler = null)
		{
			Identifier = identifier;
			Handler = handler;
			Client = handler == null ? new HttpClient() : new HttpClient(handler);
		}

		public bool HasAuthenticated { get; private set; }

		TaskCompletionSource<bool> authenticatingTask = new TaskCompletionSource<bool>(); 



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
				authenticatingTask.TrySetResult(true);
				OnAccountUpdated(currentAccount);
#pragma warning restore 4014
			}
		}

		Task<Account> authenticateTask;
		public async Task<Account> Authenticate(string[] scope)
		{
			if (authenticateTask != null && !authenticateTask.IsCompleted)
			{
				return await authenticateTask;
			}
			authenticateTask = PerformAuthenticate(scope);
			var result = await authenticateTask;
			if (result == null)
			{
				authenticatingTask.TrySetResult(false);
			}
			return result;
		}

		public abstract Task<Account> Authenticate();

		protected abstract Task<Account> PerformAuthenticate(string[] scope);

		protected abstract Task RefreshAccount(Account account);

		public static Action<Authenticator> ShowAuthenticator { get; set; }
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


		public async virtual Task<List<T>> GetGenericList<T>(string path)
		{
			var items = await Get<List<T>>(path);

			return items;
		}

		public virtual async Task PrepareClient(HttpClient client)
		{
			await VerifyCredentials();
		}

		public async virtual Task<Stream> GetUrlStream(string path)
		{
			//			var resp = await GetMessage (path);
			//			return await resp.Content.ReadAsStreamAsync ();
			return await Client.GetStreamAsync(new Uri(path));
		}


		public async virtual Task<string> GetString(string path)
		{
			var resp = await GetMessage(path);
			return await resp.Content.ReadAsStringAsync();
		}

		public virtual async Task<string> PostUrl(string path, string content, string mediaType = "text/json")
		{
			var message = await Client.PostAsync(path, new StringContent(content, System.Text.Encoding.UTF8, mediaType));
			return await message.Content.ReadAsStringAsync();
		}


		public virtual async Task<T> Get<T>(string path, string id = "")
		{
			var data = await GetString(Path.Combine(path, id));
			return await Task.Run(() => Deserialize<T>(data));

		}

		public virtual async Task<T> Post<T>(string path, string content)
		{
			Debug.WriteLine("{0} - {1}", path, content);
			var data = await PostUrl(path, content);
			if(Verbose)
				Debug.WriteLine(data);
			return await Task.Run(() => Deserialize<T>(data));

		}
		public virtual async Task<T> Post<T>(string path, HttpContent content)
		{
			Debug.WriteLine("{0} - {1}", path, await content.ReadAsStringAsync());
			var resp = await PostMessage(path, content);
			var data = await resp.Content.ReadAsStringAsync();
			//if(Verbose)
				Debug.WriteLine(data);
			return await Task.Run(() => Deserialize<T>(data));

		}
		public async Task<HttpResponseMessage> PostMessage(string path, HttpContent content)
		{
			await VerifyCredentials();
			return await Client.PostAsync(path, content);
		}

		public async Task<HttpResponseMessage> PutMessage(string path, HttpContent content)
		{
			await VerifyCredentials();
			return await Client.PutAsync(path, content);
		}

		public async Task<HttpResponseMessage> GetMessage(string path)
		{
			await VerifyCredentials();
            return await Client.GetAsync(path);
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
	}
}

