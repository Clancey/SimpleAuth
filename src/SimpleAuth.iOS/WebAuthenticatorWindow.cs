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
using SimpleAuth.iOS;
using System.Threading.Tasks;
namespace SimpleAuth
{
	public class WebAuthenticatorWindow : UIWindow
	{
		
		public WebAuthenticatorWindow() : this(UIScreen.MainScreen)
		{

		}
		public WebAuthenticatorWindow(UIScreen screen) : base(screen.Bounds)
		{

		}
		static WebAuthenticatorWindow shared;
		static WebAuthenticatorWindow Shared {
			get => shared ?? (shared = new WebAuthenticatorWindow());
			set => shared = value;
		}
		public static void PresentAuthenticator(WebAuthenticator authenticator)
		{
			var invoker = new Foundation.NSObject();
			invoker.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					var vc = new iOS.WebAuthenticatorViewController(authenticator);
					await Shared.Show(vc);
				}
				catch (Exception ex)
				{
					authenticator.OnError(ex.Message);
				}
			});
		}

		UIWindow previousKeyWindow;
		public async Task Show(WebAuthenticatorViewController authenticator)
		{
			if (!this.IsKeyWindow)
			{
				previousKeyWindow = UIKit.UIApplication.SharedApplication.KeyWindow;
				this.RootViewController = new UIViewController();
				this.MakeKeyAndVisible();
			}
			authenticator.Dismiss = async ()=> await Dismiss();
			await this.RootViewController.PresentViewControllerAsync(new UINavigationController(authenticator), true);
		}
		public async Task Dismiss()
		{
			await this.RootViewController.DismissViewControllerAsync(true);
			previousKeyWindow?.MakeKeyAndVisible();
			this.RootViewController = null;
			this.Hidden = true;
			this.RemoveFromSuperview();
		}
	}
}
