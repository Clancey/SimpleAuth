using SimpleAuth.Providers;
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
			var url = await authenticator.GetInitialUrl();

            // Thw Windows WebAuthenticatorBroker uses the IE browser which doesn't behave well
            // for at least the Instagram authenticator. So, here we're using a custom borker.
            // https://peterfoot.net/2019/03/13/webauthenticationbroker-and-github/
            if (authenticator is InstagramAuthenticator)
            {
                // result here is of type SimpleAuth.UWP.WebAuthenticationResult
                var result = await CustomWebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, url, authenticator.RedirectUrl);
                if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                {
                    authenticator.OnCancelled();
                    return;
                }
                if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                    throw new Exception(result.ResponseErrorDetail.ToString());
                authenticator.CheckUrl(new Uri(result.ResponseData), new System.Net.Cookie[0]);
            }
            else
            {
                // result here is of type Windows.Security.Authentication.Web.WebAuthenticationResult
                var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, url, authenticator.RedirectUrl);
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
}
