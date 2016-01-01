using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace SimpleAuth.UWP
{
	class WebAuthenticatorWebView
	{
		private WebAuthenticator authenticator;

		public WebAuthenticatorWebView(WebAuthenticator authenticator)
		{
			this.authenticator = authenticator;
		}

		public async Task ShowAsync()
		{
			var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, await authenticator.GetInitialUrl(), authenticator.RedirectUrl);
			if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
			{
				authenticator.OnCancelled();
				return;
			}
			if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
				throw new Exception(result.ResponseErrorDetail.ToString());
			authenticator.CheckUrl(new Uri(result.ResponseData), new System.Net.Cookie[0]);
		}
	}
}
