using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuth
{
    public abstract class AuthenticatedApi : Api
    {
		/// <summary>
		/// With this enabled, no need to call Authenticate. It will be automatically called when an Authenticated api call is made
		/// </summary>
		public bool AutoAuthenticate { get; set; } = true;

		public AuthenticatedApi(string identifier, HttpMessageHandler handler = null) : base(identifier, handler)
		{

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
				HasAuthenticated = value!= null;
#pragma warning disable 4014
				if (value == null)
					ResetClient(Client);
				else
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

		public override void ResetData()
		{
			CurrentAccount = null;
			base.ResetData();
		}

		protected virtual void SaveAccount(Account account)
		{
			AuthStorage.SetSecured(account.Identifier, SerializeObject(account), ClientId, ClientSecret, SharedGroupAccess);
		}

		protected virtual T GetAccount<T>(string identifier) where T : Account
		{
			try
			{
				var data = AuthStorage.GetSecured(identifier, ClientId, ClientSecret, SharedGroupAccess);
				return string.IsNullOrWhiteSpace(data) ? null : Deserialize<T>(data);
			}
			catch (Exception ex)
			{
				OnException(this, ex);
				return null;
			}
		}

		protected override async Task VerifyCredentials()
		{
			if (CurrentAccount == null)
			{
				if (AutoAuthenticate)
					await Authenticate();
				else
					throw new Exception("Not Authenticated");
			}
			if (!CurrentAccount.IsValid())
				await RefreshAccount(CurrentAccount);
		}

		protected override Task InvalidateCredentials ()
		{
			CurrentAccount?.Invalidate ();
			return Task.FromResult (true);
		}
	}
}
