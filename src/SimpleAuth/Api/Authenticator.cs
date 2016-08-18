using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth
{
	public abstract class Authenticator
	{

		public string AuthCode
		{
			get;
			private set;
		}

		public Authenticator()
		{
			AllowsCancel = true;
			Title = "Sign in";
		}

		public string Title
		{
			get;
			set;
		}

		protected TaskCompletionSource<string> tokenTask = new TaskCompletionSource<string> ();

		public Task<string> GetAuthCode()
		{
			return tokenTask.Task;
		}

		public bool AllowsCancel
		{
			get;
			set;
		}

		public void OnCancelled()
		{
			HasCompleted = true;
			tokenTask?.TrySetCanceled();
		}
		
		protected void FoundAuthCode(string authCode)
		{
			HasCompleted = !string.IsNullOrWhiteSpace(authCode);
			AuthCode = authCode;
			tokenTask?.TrySetResult(authCode);
		}

		public void OnError(string error)
		{
			if(!HasCompleted)
				tokenTask?.TrySetException(new Exception(error));
		}

		public string ClientId
		{
			get;
			set;
		}

		public bool HasCompleted
		{
			get;
			private set;
		}

	}
	
}
