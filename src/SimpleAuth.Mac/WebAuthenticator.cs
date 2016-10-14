﻿using System;
using WebKit;
using AppKit;
using Foundation;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace SimpleAuth.Mac
{
	public class WebAuthenticatorWebView : WebKit.WebView, IWebFrameLoadDelegate
	{
		public readonly WebAuthenticator Authenticator;
		public WebAuthenticatorWebView(WebAuthenticator authenticator)
		{
			this.Authenticator = authenticator;
			MonitorAuthenticator ();
			this.FrameLoadDelegate = this;
		}

		async Task MonitorAuthenticator ()
		{
			try {
				await Authenticator.GetAuthCode ();
				if (!Authenticator.HasCompleted)
					return;
				BeginInvokeOnMainThread (() => {
					try {
						var app = NSApplication.SharedApplication;
						app.EndSheet (shownInWindow);
						window.OrderOut (this);
					} catch (Exception ex) {
						Console.WriteLine (ex);
					}
				});
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		Task loadingTask;

		public async void BeginLoadingInitialUrl ()
		{
			if (loadingTask == null || loadingTask.IsCompleted) {
				loadingTask = RealLoading ();
			}
			await loadingTask;
			
		}

		async Task RealLoading ()
		{
			//activity.StartAnimating ();
			ClearCookies ();
			if (!this.Authenticator.ClearCookiesBeforeLogin)
				LoadCookies ();

			//
			// Begin displaying the page
			//
			try {
				var url = await Authenticator.GetInitialUrl ();
				if (url == null)
					return;
				var request = new NSUrlRequest (new NSUrl (url.AbsoluteUri));
				NSUrlCache.SharedCache.RemoveCachedResponse (request);
				this.MainFrame.LoadRequest (request);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return;
			} finally {
				//activity.StopAnimating ();
			}
		}

		void LoadCookies ()
		{
			Authenticator?.Cookies?.ToList ()?.ForEach (x => NSHttpCookieStorage.SharedStorage.SetCookie (new NSHttpCookie (x.Name, x.Value, x.Path, x.Domain)));
		}
		void ClearCookies ()
		{
			var cookies = NSHttpCookieStorage.SharedStorage.Cookies.ToList ();
			cookies.ForEach (NSHttpCookieStorage.SharedStorage.DeleteCookie);
		}

		private Cookie[] GetCookies (string url)
		{
			var store = NSHttpCookieStorage.SharedStorage;
			var cookies = store.CookiesForUrl (new NSUrl (url)).Select (x => new Cookie (x.Name, x.Value, x.Path, x.Domain)).ToArray ();
			return cookies;
		}

		[Export ("webView:didStartProvisionalLoadForFrame:")]
		public void StartedProvisionalLoad (WebKit.WebView sender, WebKit.WebFrame forFrame)
		{
			var url = sender.MainFrameUrl;

			if (!Authenticator.HasCompleted) {
				Uri uri;
				if (Uri.TryCreate (url, UriKind.Absolute, out uri)) {
					if (Authenticator.CheckUrl (uri, GetCookies (url)))
						return;
				}
			}
		}

		[Foundation.Export ("webView:didFinishLoadForFrame:")]
		public void FinishedLoad (WebView sender, WebFrame forFrame)
		{
			var url = sender.MainFrameUrl;

			if (!Authenticator.HasCompleted) {
				Uri uri;
				if (Uri.TryCreate (url, UriKind.Absolute, out uri)) {
					if (Authenticator.CheckUrl (uri, GetCookies (url)))
						return;
				}
			}
		}

		[Foundation.Export ("webView:didFailLoadWithError:forFrame:")]
		public void FailedLoadWithError (WebView sender, NSError error, WebFrame forFrame)
		{
			Authenticator.OnError (error.LocalizedDescription);
		}

		[Foundation.Export ("webView:didFailProvisionalLoadWithError:forFrame:")]
		public void FailedProvisionalLoad (WebView sender, NSError error, WebFrame forFrame)
		{
			var url = sender.MainFrameUrl;

			if (!Authenticator.HasCompleted) {
				Uri uri;
				if (Uri.TryCreate (url, UriKind.Absolute, out uri)) {
					if (Authenticator.CheckUrl (uri, GetCookies (url))) {
						forFrame.StopLoading ();
						return;
					}
				}
			}
			Authenticator.OnError (error.LocalizedDescription);
		}

		[Export ("webView:willPerformClientRedirectToURL:delay:fireDate:forFrame:")]
		public void WillPerformClientRedirect (WebKit.WebView sender, Foundation.NSUrl toUrl, double secondsDelay, Foundation.NSDate fireDate, WebKit.WebFrame forFrame)
		{
			var url = sender.MainFrameUrl;

			if (!Authenticator.HasCompleted) {
				Uri uri;
				if (Uri.TryCreate (url, UriKind.Absolute, out uri)) {
					if (Authenticator.CheckUrl (uri, GetCookies (url))) {
						forFrame.StopLoading ();
						return;
					}
				}
			} else {
				forFrame.StopLoading ();
			}
		}

		static NSWindow window;
		static NSWindow shownInWindow;
		public static async void ShowWebivew(WebAuthenticatorWebView webview)
		{
			var app = NSApplication.SharedApplication;
			var rect = new CoreGraphics.CGRect (0, 0, 400, 600);
			webview.Frame = rect;
			window = new NSWindow (rect, NSWindowStyle.Closable | NSWindowStyle.Titled, NSBackingStore.Buffered, false);
			window.ContentView = webview;
			window.IsVisible = false;
			window.Title = webview.Authenticator.Title;
			while (shownInWindow == null) {
				shownInWindow = app.MainWindow;
				if (shownInWindow == null)
					await Task.Delay (1000);
			}
			app.BeginSheet (window, shownInWindow);
			webview.BeginLoadingInitialUrl ();
		}
	}
}

