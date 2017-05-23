//
//  Copyright 2017  (c) James Clancey
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
using System;
using Google.SignIn;
using Foundation;
using SimpleAuth;
using UIKit;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace SimpleAuth.Providers
{
	public static class Google
	{
		public static void Init ()
		{
			GoogleApi.IsUsingNative = true;
			GoogleApi.GoogleShowAuthenticator = Login;
			GoogleApi.OnLogOut = LogOut;
			Native.RegisterCallBack ("google",OpenUrl);
		}

		public static Action<SignIn, UIViewController> PresentViewController;
		public static Action<SignIn, UIViewController> DismissViewController;
		static NativeHandler currentHandler;
		public static bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
		{
			var openUrlOptions = new UIApplicationOpenUrlOptions (options);
			if (currentHandler != null)
				return currentHandler.SignIn.HandleUrl (url, openUrlOptions.SourceApplication, options);
			return SignIn.SharedInstance.HandleUrl (url, openUrlOptions.SourceApplication, options);
		}

		static async void Login (WebAuthenticator authenticator)
		{
			currentHandler?.Cancel ();
			try {
				currentHandler = new NativeHandler ();

				var gAuth = authenticator as GoogleAuthenticator;
				var user = await currentHandler.Authenticate (gAuth);
				gAuth.ServerToken = user.ServerAuthCode;
				gAuth.IdToken = user.Authentication.IdToken;
				gAuth.RefreshToken = user.Authentication.RefreshToken;
				gAuth.OnRecievedAuthCode (user.Authentication.AccessToken);

			} catch (TaskCanceledException) {
				authenticator?.OnCancelled ();
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
				authenticator.OnError (ex.Message);
			}




		}
		static async void LogOut (string clientId, string clientsecret)
		{
			SignIn.SharedInstance.ClientID = clientId;
			SignIn.SharedInstance.SignOutUser ();
		}




		class NativeHandler : SignInDelegate,ISignInUIDelegate
		{
			WeakReference authenticatorReference;

			public readonly SignIn SignIn = new SignIn ();
			TaskCompletionSource<GoogleUser> tcs = new TaskCompletionSource<GoogleUser> ();
			public async Task<GoogleUser> Authenticate (GoogleAuthenticator authenticator)
			{
				authenticatorReference = new WeakReference (authenticator);
				SignIn.ClientID = GoogleAuthenticator.GetGoogleClientId (authenticator.ClientId);
				SignIn.ServerClientID = GoogleAuthenticator.GetGoogleClientId(authenticator.ServerClientId);
				SignIn.Scopes = authenticator.Scope.ToArray ();
				SignIn.Delegate = this;
				SignIn.UIDelegate = this;
				SignIn.SignInUser ();
				return await tcs.Task;
			}

			public void Cancel ()
			{
				tcs?.TrySetCanceled ();
			}
			public override void DidSignIn (SignIn signIn, GoogleUser user, NSError error)
			{
				if (user != null && error == null) {
					tcs?.TrySetResult (user);
				} else {
					tcs.TrySetException (new Exception (error.Description));
				}
			}
			public override void DidDisconnect (SignIn signIn, GoogleUser user, NSError error)
			{
				base.DidDisconnect (signIn, user, error);
			}

			[Export ("signInWillDispatch:error:")]
			public void WillDispatch (SignIn signIn, NSError error)
			{
				
			}

			[Export ("signIn:presentViewController:")]
			public void PresentViewController (SignIn signIn, UIViewController viewController)
			{
				if (Google.PresentViewController != null) {
					Google.PresentViewController (SignIn, viewController);
					return;
				}

				var window = UIApplication.SharedApplication.KeyWindow;
				var root = window.RootViewController;
				if (root != null) {
					var current = root;
					while (current.PresentedViewController != null) {
						current = current.PresentedViewController;
					}
					var authenticator = (authenticatorReference.Target as GoogleAuthenticator);
					viewController.Title = authenticator.Title;
					if (authenticator.AllowsCancel) {
						viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
							UIBarButtonSystemItem.Cancel,
							delegate {
								Cancel ();
								viewController.DismissViewController (true,null);
							});
					}
					current.PresentViewControllerAsync (new UIKit.UINavigationController (viewController), true);
				}
			}

			[Export ("signIn:dismissViewController:")]
			public void DismissViewController (SignIn signIn, UIViewController viewController)
			{
				if (Google.DismissViewController != null) {
					Google.DismissViewController (SignIn, viewController);
					return;
				}

				viewController.DismissViewController (true,null);
			}
			protected override void Dispose (bool disposing)
			{
				if (disposing) {
					SignIn.Dispose ();
				}
				base.Dispose (disposing);
			}
		}
	}
}
