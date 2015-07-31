//
//  Copyright 2012-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Android.App;
using Android.Net.Http;
using Android.Webkit;
using Android.OS;
using System.Threading.Tasks;
using SimpleAuth.Droid;

namespace SimpleAuth
{
	[Activity(Label = "Web Authenticator")]
#if XAMARIN_AUTH_INTERNAL
	internal class WebAuthenticatorActivity : Activity
#else
	public class WebAuthenticatorActivity : Activity
#endif
	{
		WebView webView;

		internal class State : Java.Lang.Object
		{
			public Authenticator Authenticator;
		}
		internal static readonly ActivityStateRepository<State> StateRepo = new ActivityStateRepository<State>();

		State state;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			//
			// Load the state either from a configuration change or from the intent.
			//
			state = LastNonConfigurationInstance as State;
			if (state == null && Intent.HasExtra("StateKey"))
			{
				var stateKey = Intent.GetStringExtra("StateKey");
				state = StateRepo.Remove(stateKey);
			}
			if (state == null)
			{
				Finish();
				return;
			}

			MonitorAuthenticator();
			Title = state.Authenticator.Title;
			//
			// Build the UI
			//
			webView = new WebView(this)
			{
				Id = 42,
			};
			webView.Settings.JavaScriptEnabled = true;
			webView.SetWebViewClient(new Client(this));
			SetContentView(webView);

			//
			// Restore the UI state or start over
			//
			if (savedInstanceState != null)
			{
				webView.RestoreState(savedInstanceState);
			}
			else
			{
				if (Intent.GetBooleanExtra("ClearCookies", true))
					ClearCookies();

				BeginLoadingInitialUrl();
			}
		}
		async Task MonitorAuthenticator()
		{
			try
			{
				await state.Authenticator.GetAuthCode();
				if (state.Authenticator.HasCompleted)
				{
					SetResult(Result.Ok);
					Finish();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		Task loadingTask;
		async Task BeginLoadingInitialUrl()
		{
			if (state.Authenticator.ClearCookiesBeforeLogin)
				ClearCookies();
			if (loadingTask == null || loadingTask.IsCompleted)
			{
				loadingTask = RealLoading();
			}
			await loadingTask;

		}

		async Task RealLoading()
		{
			if (this.state.Authenticator.ClearCookiesBeforeLogin)
				ClearCookies();

			//
			// Begin displaying the page
			//
			try
			{
				var url = await state.Authenticator.GetInitialUrl();
				if (url == null)
					return;
				webView.LoadUrl(url.AbsoluteUri);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return;
			}
		}

		public void ClearCookies()
		{
			Android.Webkit.CookieSyncManager.CreateInstance(Android.App.Application.Context);
			Android.Webkit.CookieManager.Instance.RemoveAllCookie();
		}
		public override void OnBackPressed()
		{
			Finish();
			state.Authenticator.OnCancelled();
		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance()
		{
			return state;
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			webView.SaveState(outState);
		}

		void BeginProgress(string message)
		{
			webView.Enabled = false;
		}

		void EndProgress()
		{
			webView.Enabled = true;
		}

		class Client : WebViewClient
		{
			WebAuthenticatorActivity activity;
			HashSet<SslCertificate> sslContinue;
			Dictionary<SslCertificate, List<SslErrorHandler>> inProgress;

			public Client(WebAuthenticatorActivity activity)
			{
				this.activity = activity;
			}

			public override bool ShouldOverrideUrlLoading(WebView view, string url)
			{
				return false;
			}

			public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
			{
				var uri = new Uri(url);
				activity.state.Authenticator.CheckUrl(uri, GetCookies(url));
				activity.BeginProgress(uri.Authority);
			}

			public override void OnPageFinished(WebView view, string url)
			{
				var uri = new Uri(url);
				activity.state.Authenticator.CheckUrl(uri,GetCookies(url));
				activity.EndProgress();
			}

			System.Net.Cookie[] GetCookies(string url)
			{
				Android.Webkit.CookieSyncManager.CreateInstance(Android.App.Application.Context);
				var cookiePairs = CookieManager.Instance.GetCookie(url).Split('&');

				return cookiePairs.Select(x =>
				{
					var parts = x.Split(':');
					return new Cookie
					{
						Name = parts[0],
						Value = parts[1],
					};
				}).ToArray();
			}

			class SslCertificateEqualityComparer
				: IEqualityComparer<SslCertificate>
			{
				public bool Equals(SslCertificate x, SslCertificate y)
				{
					return Equals(x.IssuedTo, y.IssuedTo) && Equals(x.IssuedBy, y.IssuedBy) && x.ValidNotBeforeDate.Equals(y.ValidNotBeforeDate) && x.ValidNotAfterDate.Equals(y.ValidNotAfterDate);
				}

				bool Equals(SslCertificate.DName x, SslCertificate.DName y)
				{
					if (ReferenceEquals(x, y))
						return true;
					if (ReferenceEquals(x, y) || ReferenceEquals(null, y))
						return false;
					return x.GetDName().Equals(y.GetDName());
				}

				public int GetHashCode(SslCertificate obj)
				{
					unchecked
					{
						int hashCode = GetHashCode(obj.IssuedTo);
						hashCode = (hashCode * 397) ^ GetHashCode(obj.IssuedBy);
						hashCode = (hashCode * 397) ^ obj.ValidNotBeforeDate.GetHashCode();
						hashCode = (hashCode * 397) ^ obj.ValidNotAfterDate.GetHashCode();
						return hashCode;
					}
				}

				int GetHashCode(SslCertificate.DName dname)
				{
					return dname.GetDName().GetHashCode();
				}
			}

			public override void OnReceivedSslError(WebView view, SslErrorHandler handler, SslError error)
			{
				if (sslContinue == null)
				{
					var certComparer = new SslCertificateEqualityComparer();
					sslContinue = new HashSet<SslCertificate>(certComparer);
					inProgress = new Dictionary<SslCertificate, List<SslErrorHandler>>(certComparer);
				}

				List<SslErrorHandler> handlers;
				if (inProgress.TryGetValue(error.Certificate, out handlers))
				{
					handlers.Add(handler);
					return;
				}

				if (sslContinue.Contains(error.Certificate))
				{
					handler.Proceed();
					return;
				}

				inProgress[error.Certificate] = new List<SslErrorHandler>();

				AlertDialog.Builder builder = new AlertDialog.Builder(this.activity);
				builder.SetTitle("Security warning");
				builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
				builder.SetMessage("There are problems with the security certificate for this site.");

				builder.SetNegativeButton("Go back", (sender, args) => {
					UpdateInProgressHandlers(error.Certificate, h => h.Cancel());
					handler.Cancel();
				});

				builder.SetPositiveButton("Continue", (sender, args) => {
					sslContinue.Add(error.Certificate);
					UpdateInProgressHandlers(error.Certificate, h => h.Proceed());
					handler.Proceed();
				});

				builder.Create().Show();
			}

			void UpdateInProgressHandlers(SslCertificate certificate, Action<SslErrorHandler> update)
			{
				List<SslErrorHandler> inProgressHandlers;
				if (!this.inProgress.TryGetValue(certificate, out inProgressHandlers))
					return;

				foreach (SslErrorHandler sslErrorHandler in inProgressHandlers)
					update(sslErrorHandler);

				inProgressHandlers.Clear();
			}
		}
	}
}
