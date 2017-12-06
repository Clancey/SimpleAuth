using System;
using WebKit;
using AppKit;
using Foundation;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using CoreGraphics;

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
						app.StopModal();
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
			window = new ModalWindow(webview, rect);
			while (shownInWindow == null) {
				shownInWindow = app.MainWindow;
				if (shownInWindow == null)
					await Task.Delay (1000);
			}

			webview.BeginLoadingInitialUrl();
			app.RunModalForWindow(window);
			//app.BeginSheet (window, shownInWindow);
		}
		class ModalWindow : NSWindow, INSWindowDelegate
		{
			static NSWindowStyle GetStyle(WebAuthenticatorWebView webView) => webView.Authenticator.AllowsCancel ? NSWindowStyle.Closable | NSWindowStyle.Titled : NSWindowStyle.Titled;
			WeakReference webview;
			public ModalWindow(WebAuthenticatorWebView webView, CGRect rect) : base(rect,GetStyle(webView), NSBackingStore.Buffered, false)
			{
				webview = new WeakReference(webView);
				webView.Frame = rect;
				ContentView = webView;
				IsVisible = false;
				Title = webView.Authenticator.Title;
				this.Delegate = this;
			}
			[Export("windowWillClose:")]
			public void WindowWillClose(NSNotification notification)
			{
				var auth = (webview?.Target as WebAuthenticatorWebView)?.Authenticator;
				if (!auth.HasCompleted)
					auth.OnCancelled();
				NSApplication.SharedApplication.StopModal();
			}
		}
	}
}

