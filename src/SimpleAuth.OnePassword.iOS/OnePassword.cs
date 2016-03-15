using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace SimpleAuth
{
	public static class OnePassword
	{
		static Task loadingTask;
		public static void Activate()
		{
			if (!AgileBits.OnePasswordExtension.SharedExtension.IsAppExtensionAvailable)
				return;
			SimpleAuth.iOS.WebAuthenticatorViewController.RightButtonItem = new UIBarButtonItem(UIImage.FromBundle("onepassword-navbar.png"),UIBarButtonItemStyle.Plain,
				async (s, e) =>
				{
					await WaitForLoadingToEnd();
					//iOS has a horrible crash and burn issue that this fixes
					var tintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
					UIApplication.SharedApplication.KeyWindow.TintColor = null;
                    AgileBits.OnePasswordExtension.SharedExtension.FillLoginIntoWebView(iOS.WebAuthenticatorViewController.Shared.webView, iOS.WebAuthenticatorViewController.Shared,SimpleAuth.iOS.WebAuthenticatorViewController.RightButtonItem,
						(success, error) =>
						{
							if(error != null)
								Console.WriteLine(error);
							if (tintColor != null)
								UIApplication.SharedApplication.KeyWindow.TintColor = tintColor;
						});
				});
		}

		static async Task WaitForLoadingToEnd()
		{
			var webView = iOS.WebAuthenticatorViewController.Shared.webView;
			if (webView.IsLoading)
				return;
			await Task.Run(async () =>
			{
				while (true)
				{
					await Task.Delay(1000);
					if (!webView.IsLoading)
						return;
				}
			});
		}
	}
}
