using System;
using UIKit;
using CoreGraphics;
using Foundation;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace SimpleAuth.iOS
{
	public class WebAuthenticator : UIViewController
	{

		public readonly Authenticator Authenticator;


		UIWebView webView;
		UIActivityIndicatorView activity;
		UIView authenticatingView;
		ProgressLabel progress;
		bool webViewVisible = true;

		const double TransitionTime = 0.25;

		bool keepTryingAfterError = true;

		public WebAuthenticator (Authenticator authenticator)
		{
			this.Authenticator = authenticator;
			MonitorAuthenticator ();
			//
			// Create the UI
			//
			Title = authenticator.Title;

			if (authenticator.AllowsCancel) {
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
					UIBarButtonSystemItem.Cancel,
					delegate {
						Cancel ();
					});				
			}

			var activityStyle = UIActivityIndicatorViewStyle.White;
			if (UIDevice.CurrentDevice.CheckSystemVersion (7, 0))
				activityStyle = UIActivityIndicatorViewStyle.Gray;

			activity = new UIActivityIndicatorView (activityStyle);
			NavigationItem.RightBarButtonItems = new [] {

				#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				new UIBarButtonItem (UIBarButtonSystemItem.Refresh, (s, e) => BeginLoadingInitialUrl ()),
				#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				new UIBarButtonItem (activity),
			};

			webView = new UIWebView (View.Bounds) {
				Delegate = new WebViewDelegate (this),
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
			};
			View.AddSubview (webView);
			View.BackgroundColor = UIColor.Black;

			//
			// Locate our initial URL
			//
			#pragma warning disable 4014
			BeginLoadingInitialUrl ();
			#pragma warning restore 4014
		}

		async Task MonitorAuthenticator ()
		{
			try{
				await Authenticator.GetAuthCode ();
				if (Authenticator.HasCompleted)
					await this.DismissViewControllerAsync (true);
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
		}

		void Cancel ()
		{
			this.DismissViewControllerAsync (true);
			Authenticator.OnCancelled ();
		}

		static void ClearCookies ()
		{
			foreach(var cookie in NSHttpCookieStorage.SharedStorage.Cookies)
				NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);	
		}

		Task loadingTask;

		async Task BeginLoadingInitialUrl ()
		{
			if (this.Authenticator.ClearCookiesBeforeLogin)
				ClearCookies ();
			if (loadingTask == null || loadingTask.IsCompleted) {
				loadingTask = RealLoading ();
			}
			await loadingTask;


		}

		async Task RealLoading ()
		{
			activity.StartAnimating ();
			if (this.Authenticator.ClearCookiesBeforeLogin)
				ClearCookies ();

			//
			// Begin displaying the page
			//
			try {
				var url = await Authenticator.GetInitialUrl ();
				if (url == null)
					return;
				LoadInitialUrl (url);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return;
			} finally {
				activity.StopAnimating ();
			}
		}

		void LoadInitialUrl (Uri url)
		{
			if (!webViewVisible) {
				progress.StopAnimating ();
				webViewVisible = true;
				UIView.Transition (
					fromView: authenticatingView,
					toView: webView,
					duration: TransitionTime,
					options: UIViewAnimationOptions.TransitionCrossDissolve,
					completion: null);
			}

			if (url == null)
				return;

			var request = new NSUrlRequest (new NSUrl (url.AbsoluteUri));
			NSUrlCache.SharedCache.RemoveCachedResponse (request); // Always try
			webView.LoadRequest (request);
		}

		void HandleBrowsingCompleted (object sender, EventArgs e)
		{
			activity.StopAnimating ();
			if (!webViewVisible)
				return;

			if (authenticatingView == null) {
				authenticatingView = new UIView (View.Bounds) {
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					BackgroundColor = UIColor.FromRGB (0x33, 0x33, 0x33),
				};
				progress = new ProgressLabel ("Authenticating...");
				var f = progress.Frame;
				var b = authenticatingView.Bounds;
				f.X = (b.Width - f.Width) / 2;
				f.Y = (b.Height - f.Height) / 2;
				progress.Frame = f;
				authenticatingView.Add (progress);
			} else {
				authenticatingView.Frame = View.Bounds;
			}

			webViewVisible = false;

			progress.StartAnimating ();

			UIView.Transition (
				fromView: webView,
				toView: authenticatingView,
				duration: TransitionTime,
				options: UIViewAnimationOptions.TransitionCrossDissolve,
				completion: null);
		}

		protected class WebViewDelegate : UIWebViewDelegate
		{
			WeakReference controller;

			protected WebAuthenticator Controller {
				get{ return controller == null ? null : controller.Target as WebAuthenticator; }
				set { controller = new WeakReference (value); }
			}

			Uri lastUrl;

			public WebViewDelegate (WebAuthenticator controller)
			{
				this.Controller = controller;
			}

			public override bool ShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
			{
				var nsUrl = request.Url;

				if (nsUrl != null && !Controller.Authenticator.HasCompleted) {
					Uri url;
					if (Uri.TryCreate (nsUrl.AbsoluteString, UriKind.Absolute, out url)) {
						if (Controller.Authenticator.CheckUrl (url, GetCookies (url)))
							return false;
					}
				}

				return true;
			}

			public override void LoadStarted (UIWebView webView)
			{
				Controller.activity.StartAnimating ();

				webView.UserInteractionEnabled = false;
			}

			public override void LoadFailed (UIWebView webView, NSError error)
			{
				if (error.Domain == "NSURLErrorDomain" && error.Code == -999)
					return;

				Controller.activity.StopAnimating ();

				webView.UserInteractionEnabled = true;

				Controller.Authenticator.OnError (error.LocalizedDescription);
			}

			public override void LoadingFinished (UIWebView webView)
			{
				Controller.activity.StopAnimating ();

				webView.UserInteractionEnabled = true;

				var url = new Uri (webView.Request.Url.AbsoluteString);
				if (url != lastUrl && !Controller.Authenticator.HasCompleted) {
					lastUrl = url;
					Controller.Authenticator.CheckUrl (url, GetCookies (url));
				}
			}

			private Cookie[] GetCookies (Uri url)
			{
				var store = NSHttpCookieStorage.SharedStorage;
				var cookies = store.CookiesForUrl (new NSUrl (url.AbsoluteUri)).Select (x => new Cookie (x.Name, x.Value, x.Path, x.Domain)).ToArray ();
				return cookies;
			}
		}
	}

	internal class ProgressLabel : UIView
	{
		UIActivityIndicatorView activity;

		public ProgressLabel (string text)
			: base (new CGRect (0, 0, 200, 44))
		{
			BackgroundColor = UIColor.Clear;

			activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White) {
				Frame = new CGRect (0, 11.5f, 21, 21),
				HidesWhenStopped = false,
				Hidden = false,
			};
			AddSubview (activity);

			var label = new UILabel () {
				Text = text,
				TextColor = UIColor.White,
				Font = UIFont.BoldSystemFontOfSize (20),
				BackgroundColor = UIColor.Clear,
				Frame = new CGRect (25, 0, Frame.Width - 25, 44),
			};
			AddSubview (label);

			var f = Frame;
			f.Width = label.Frame.X + UIStringDrawing.StringSize (label.Text, label.Font).Width;
			Frame = f;
		}

		public void StartAnimating ()
		{
			activity.StartAnimating ();
		}

		public void StopAnimating ()
		{
			activity.StopAnimating ();
		}
	}
}
