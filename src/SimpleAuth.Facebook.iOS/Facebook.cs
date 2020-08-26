using System;
using fb = Facebook;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using UIKit;
using Foundation;
using System.Linq;
using Facebook.LoginKit;

namespace SimpleAuth.Providers
{
	public static class Facebook
	{
		static Foundation.NSObject invoker = new Foundation.NSObject();
		public static void Init(UIKit.UIApplication app, Foundation.NSDictionary launchOptions)
		{
			FacebookApi.IsUsingNative = true;
			fb.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(app, launchOptions);
			FacebookApi.ShowFacebookAuthenticator = (a) => invoker.BeginInvokeOnMainThread(() => Login(a));
			Native.RegisterCallBack ("facebook", Callback);
			FacebookApi.OnLogOut = Logout;
		}

		private static bool Callback(UIApplication application, NSUrl url, NSDictionary options)
		{
			NSDictionary<NSString, NSObject> converted = new NSDictionary<NSString, NSObject>(
				   options.Keys.Select(x => x as NSString).ToArray(),
				   options.Values);

			return OpenUrl(application, url, converted);
		}

		public static bool OpenUrl(UIApplication app, NSUrl url, string sourceApp, NSObject annotation)
		{
			return fb.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(app, url, sourceApp, annotation);
		}

		public static bool OpenUrl (UIApplication app, NSUrl url, NSDictionary<NSString,NSObject> options)
		{
			return fb.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl (app, url, options);
		}

		public static async void Login (WebAuthenticator authenticator)
		{
			var fbAuth = authenticator as FacebookAuthenticator;
			try
			{
				fb.CoreKit.Settings.AppId = fbAuth.ClientId;
				var loginManager = new fb.LoginKit.LoginManager();
				var window = UIKit.UIApplication.SharedApplication.KeyWindow;
				var root = window.RootViewController;
				if (root != null)
				{
					var current = root;
					while (current.PresentedViewController != null)
					{
						current = current.PresentedViewController;
					}

					var tcs = new TaskCompletionSource<LoginManagerLoginResult> ();
					loginManager.LogIn (authenticator.Scope.ToArray (), current, (LoginManagerLoginResult result, NSError error) => {
						if (error != null)
							tcs.TrySetException (new Exception (error.LocalizedDescription));
						else
							tcs.SetResult (result);
					});

					var resp = await tcs.Task;
					if (resp.IsCancelled)
					{
						authenticator.OnCancelled();
						return;
					}
					var date = (DateTime)resp.Token.ExpirationDate;
					var expiresIn = (long)(date - DateTime.Now).TotalSeconds;
					fbAuth.OnRecievedAuthCode(resp.Token.TokenString,expiresIn);
				}
			}
			catch (Exception ex)
			{
				authenticator.OnError(ex.Message);
			}
		}
		public static void  Logout()
		{
			var manager = new fb.LoginKit.LoginManager();
			manager.LogOut();
		}
	}
}

