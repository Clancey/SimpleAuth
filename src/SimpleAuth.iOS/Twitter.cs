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
using System.Linq;
using System.Web;
using System.Collections.Generic;
namespace SimpleAuth.Providers
{
	public static class Twitter
	{
		static Foundation.NSObject invoker = new Foundation.NSObject();
		public static void Init()
		{

			TwitterApi.IsUsingNative = true;

			TwitterApi.ShowTwitterAuthenticator = (a, fallback) => invoker.BeginInvokeOnMainThread(() => Login(a, fallback));
			Native.RegisterCallBack("twitter", Callback);
		}

		private static bool Callback(UIApplication application, NSUrl url, NSDictionary options)
		{
			var converted = new Dictionary<string, NSObject>();
			foreach (var key in options.Keys)
				converted.Add(key.ToString(), options.ObjectForKey(key));
			foreach (var pair in converted)
			{
				Console.WriteLine($"{pair.Key} - {pair.Value.DebugDescription}");
			}
			Console.WriteLine(url);
			return OpenUrl(application, url, converted);
		}

		public static bool OpenUrl(UIApplication app, NSUrl url, Dictionary<string, NSObject> options)
		{
			try
			{
				if (!url.Scheme.StartsWith("twitterkit-", StringComparison.Ordinal))
					return false;
				var pieces = url.AbsoluteString.Split(new[] { "://" },StringSplitOptions.None);
				var id = pieces[1];
				var query = pieces[2];
				if (!CurrentAuthenticators.TryGetValue(id, out var currentAuthenticator))
					return false;
				return currentAuthenticator.CheckNativeUrl(query);
			}
			catch (Exception ex)
			{
	
			}
			return false;
		}
		static Dictionary<string, TwitterAuthenticator> CurrentAuthenticators = new Dictionary<string, TwitterAuthenticator>();
		public static async void Login(WebAuthenticator authenticator, Action<WebAuthenticator> fallback)
		{
			var ta = authenticator as TwitterAuthenticator;
			var scheme = $"twitterkit-{ta.ClientId}";
			if (!NativeSafariAuthenticator.VerifyHasUrlScheme(scheme))
			{
				authenticator.OnError($"Unable to redirect {scheme}, Please add the Url Scheme to the info.plist");
				return;
			}
			var returnUrl = HttpUtility.UrlEncode($"{scheme}://{ta.Identifier}");
			var nativeUrl = new NSUrl($"twitterauth://authorize?consumer_key={ta.ClientId}&consumer_secret={ta.ClientSecret}&oauth_callback={returnUrl}");
			if (UIApplication.SharedApplication.CanOpenUrl(nativeUrl))
			{
				if ((await UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions())))
				{
					CurrentAuthenticators[ta.Identifier] = ta;
					return;
				}

			}
			fallback(authenticator);
		}
	}
}

