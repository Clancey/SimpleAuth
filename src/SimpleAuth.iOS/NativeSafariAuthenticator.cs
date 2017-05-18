//
//  Copyright 2016  Clancey
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
using System.Collections.Generic;
using Foundation;
using System.Linq;
using SafariServices;
using UIKit;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth
{
	public class NativeSafariAuthenticator
	{
		public NativeSafariAuthenticator ()
		{
		}
		const string CFBundleUrlError = "CFBundleURLSchemes are required for Native Safari Auth";
		static Dictionary<string, WebAuthenticator> authenticators = new Dictionary<string, WebAuthenticator> ();
		static SFSafariViewController CurrentController;

		public static void Activate ()
		{
			RegisterCallbacks ();
			OAuthApi.ShowAuthenticator = ShowAuthenticator;
		}
		internal static void RegisterCallbacks ()
		{
			if (!GetCFBundleURLSchemes ().Any ())
				throw new Exception (CFBundleUrlError);
			Native.RegisterCallBack ("NativeSafariAuth",(UIApplication application, NSUrl url, NSDictionary options) => ResumeAuth (url.AbsoluteString));
		}

		public static void ShowAuthenticator (UIViewController presentingController, WebAuthenticator authenticator)
		{
			var urls = GetCFBundleURLSchemes ();
			if (!urls.Any ()) {
				authenticator.OnError (CFBundleUrlError);
				return;
			}

			//TODO: validate the proper url is in there

			var invoker = new Foundation.NSObject ();
			invoker.BeginInvokeOnMainThread (async () => await BeginAuthentication (presentingController, authenticator));
		}

		static async Task BeginAuthentication (UIViewController presentingController, WebAuthenticator authenticator)
		{
			try {
				var uri = (await authenticator.GetInitialUrl ());
				string redirectUrl = uri.GetParameter ("redirect_uri");
				var scheme = new Uri (redirectUrl).Scheme;
				if (!VerifyHasUrlScheme(scheme)) {
					authenticator.OnError ($"Unable to redirect {redirectUrl}, Please add the Url Scheme to the info.plist");
					return;
				}
				var url = new NSUrl (uri.AbsoluteUri);
				if (UIDevice.CurrentDevice.CheckSystemVersion (9, 0)) {
					var controller = new SFSafariViewController (url, false) {
						Delegate = new NativeSFSafariViewControllerDelegate (authenticator),
					};
					authenticators [scheme] = authenticator;
					CurrentController = controller;
					await presentingController.PresentViewControllerAsync (controller, true);
					return;
				}

				var opened = UIApplication.SharedApplication.OpenUrl (url);
				if (!opened)
					authenticator.OnError ("Error opening Safari");
				else
					authenticators [scheme] = authenticator;
			} catch (Exception ex) {
				authenticator.OnError (ex.Message);
			}
		}


		public static void ShowAuthenticator (WebAuthenticator authenticator)
		{
			var invoker = new Foundation.NSObject ();
			invoker.BeginInvokeOnMainThread (() => {
				var window = UIKit.UIApplication.SharedApplication.KeyWindow;
				var root = window.RootViewController;
				if (root != null) {
					var current = root;
					while (current.PresentedViewController != null) {
						current = current.PresentedViewController;
					}
					ShowAuthenticator (current, authenticator);
				}
			});
		}

		static bool VerifyHasUrlScheme (string scheme)
		{
			var schemes = GetCFBundleURLSchemes ();
			return schemes.Any (x => x == scheme);
		}
		static string [] GetCFBundleURLSchemes ()
		{
			NSObject nsobj;
			if (!NSBundle.MainBundle.InfoDictionary.TryGetValue ((NSString)"CFBundleURLTypes", out nsobj))
				return new string [0];
			var d = (nsobj as NSArray)?.GetItem<NSDictionary> (0);
			if (!d?.TryGetValue ((NSString)"CFBundleURLSchemes", out nsobj) ?? false)
				return new string [0];
			var a = nsobj as NSArray;
			var urls = ConvertToIEnumerable<NSString> (a).Select (x => x.ToString ()).ToArray ();
			return urls;
		}

		static IEnumerable<T> ConvertToIEnumerable<T> (NSArray array) where T : class, ObjCRuntime.INativeObject
		{
			for (nuint i = 0; i < array.Count; i++) {
				yield return array.GetItem<T> (i);
			}

		}
		public static bool ResumeAuth (string url)
		{
			try {
				var uri = new Uri (url);
				var redirect = uri.Scheme;
				var auth = authenticators [redirect];
				var s = auth.CheckUrl (uri, new System.Net.Cookie [0]);
				if (s) {
					authenticators.Remove (redirect);
					CurrentController?.DismissViewControllerAsync (true);
					CurrentController = null;

				}
				return s;

			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		public class NativeSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
		{
			WeakReference authenticator;
			public NativeSFSafariViewControllerDelegate (WebAuthenticator authenticator)
			{
				this.authenticator = new WeakReference (authenticator);
			}
			public override void DidFinish (SFSafariViewController controller)
			{
				(authenticator?.Target as WebAuthenticator)?.OnCancelled ();
			}
		}
	}
}
