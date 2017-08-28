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
using UIKit;
using Foundation;
namespace SimpleAuth.Providers
{
	public static class Twitter
	{
		static Foundation.NSObject invoker = new Foundation.NSObject();
		public static void Init(UIKit.UIApplication app, Foundation.NSDictionary launchOptions)
		{
			TwitterApi.ShowTwitterAuthenticator = (a) => invoker.BeginInvokeOnMainThread(() => Login(a));
			Native.RegisterCallBack("twitter", (UIApplication appplication, NSUrl url, NSDictionary options) => OpenUrl(appplication, url, options as NSDictionary<NSString, NSObject>));
		}
		public static bool OpenUrl(UIApplication app, NSUrl url, string sourceApp, NSObject annotation)
		{
			return false;
		}

		public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary<NSString, NSObject> options)
		{
			return false;
		}

		public static async void Login(WebAuthenticator authenticator)
		{
			var twitterAuth = authenticator as TwitterAuthenticator;
			try
			{

				var url = $"twitterauth://authorize?consumer_key={twitterAuth.ClientId}&consumer_secret={twitterAuth.ClientSecret}&oauth_callback=twitterkit-{twitterAuth.ClientId}";
				fb.CoreKit.Settings.AppID = fbAuth.ClientId;
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

					var resp = await loginManager.LogInWithReadPermissionsAsync(authenticator.Scope.ToArray(), current);
					if (resp.IsCancelled)
					{
						authenticator.OnCancelled();
						return;
					}
					var date = (DateTime)resp.Token.ExpirationDate;
					var expiresIn = (long)(date - DateTime.Now).TotalSeconds;
					fbAuth.OnRecievedAuthCode(resp.Token.TokenString, expiresIn);
				}
			}
			catch (Exception ex)
			{
				authenticator.OnError(ex.Message);
			}
		}
	}
}
