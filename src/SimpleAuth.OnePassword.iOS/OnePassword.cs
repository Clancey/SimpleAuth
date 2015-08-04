using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace SimpleAuth
{
	public static class OnePassword
	{
		public static void Activate()
		{
			if (!AgileBits.OnePasswordExtension.SharedExtension.IsAppExtensionAvailable)
				return;
			SimpleAuth.iOS.WebAuthenticator.RightButtonItem = new UIBarButtonItem(UIImage.FromBundle("onepassword-navbar.png"),UIBarButtonItemStyle.Plain,
				(s, e) =>
				{
					//iOS has a horrible crash and burn issue that this fixes
					var tintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
					UIApplication.SharedApplication.KeyWindow.TintColor = null;
                    AgileBits.OnePasswordExtension.SharedExtension.FillLoginIntoWebView(iOS.WebAuthenticator.Shared.webView, iOS.WebAuthenticator.Shared,SimpleAuth.iOS.WebAuthenticator.RightButtonItem,
						(success, error) =>
						{
							Console.WriteLine("Error");
						});
					if(tintColor != null)
						UIApplication.SharedApplication.KeyWindow.TintColor = tintColor;
				});
		}
	}
}
