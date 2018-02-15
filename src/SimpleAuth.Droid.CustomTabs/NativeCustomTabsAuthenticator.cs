using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.CustomTabs;
using Android.Views;
using Android.Widget;

namespace SimpleAuth
{
   public class NativeCustomTabsAuthenticator
    {
        static ActivityLifecycleCallbackManager activityLifecycleManager;
        static Dictionary<string, CustomTabsAuthSession> authenticators = new Dictionary<string, CustomTabsAuthSession>();
        public static bool IsActivated { get; private set; }
        public static void Activate(Android.App.Application app)
        {
            Native.RegisterCallBack("CustomTabs", OnActivityResult);
            OAuthApi.ShowAuthenticator = ShowAuthenticator;

            if (activityLifecycleManager == null)
            {
                activityLifecycleManager = new ActivityLifecycleCallbackManager();
                app.RegisterActivityLifecycleCallbacks(activityLifecycleManager);
            }
            IsActivated = true;
        }

		public static void OnResume()
		{
			var auths = authenticators.ToList();
			if (!auths.Any())
				return;
			foreach (var auth in auths)
			{
				auth.Value.Authenticator?.OnCancelled();
				authenticators.Remove(auth.Key);
			}
		}

        public static bool OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent intent)
        {
            if (intent == null)
                return false;

            var uri = new Uri(intent.Data.ToString());

            // Only handle schemes we expect
            var scheme = uri.Scheme;

            if (!authenticators.TryGetValue(scheme, out var authenticator))
                return false;
			if (authenticator.Authenticator.CheckUrl(uri, null))
			{
				authenticators.Remove(scheme);
				return true;
			}
			return false;
        }

        public static async void ShowAuthenticator(WebAuthenticator authenticator)
        {
            await BeginAuthentication(authenticator);
        }

        static async Task BeginAuthentication(WebAuthenticator authenticator)
        {
            try
            {
                var uri = (await authenticator.GetInitialUrl());
                string redirectUrl = uri.GetParameter("redirect_uri");
                var scheme = new Uri(redirectUrl).Scheme;

                var authSession = new CustomTabsAuthSession
                {
                    Authenticator = authenticator,
                    ParentActivity = activityLifecycleManager.CurrentActivity,
                };

                authenticators[scheme] = authSession;
                authSession.CustomTabsActivityManager = new CustomTabsActivityManager(authSession.ParentActivity);
                authSession.CustomTabsActivityManager.CustomTabsServiceConnected += delegate
                {
                    var builder = new CustomTabsIntent.Builder(authSession.CustomTabsActivityManager.Session)
                                                      .SetShowTitle(true);

                    var customTabsIntent = builder.Build();
                    customTabsIntent.Intent.AddFlags(Android.Content.ActivityFlags.SingleTop | ActivityFlags.NoHistory | ActivityFlags.NewTask);

                    CustomTabsHelper.AddKeepAliveExtra(authSession.ParentActivity, customTabsIntent.Intent);

                    customTabsIntent.LaunchUrl(authSession.ParentActivity, Android.Net.Uri.Parse(uri.AbsoluteUri));
                };

				if (!authSession.CustomTabsActivityManager.BindService())
				{
					authenticator.OnError("CustomTabs not supported.");
					authenticators.Remove(scheme);
				}

            }
            catch (Exception ex)
            {
                authenticator.OnError(ex.Message);
            }
        }
        class CustomTabsAuthSession
        {

            public CustomTabsActivityManager CustomTabsActivityManager { get; set; }
            public Activity ParentActivity { get; set; }
            public WebAuthenticator Authenticator { get; set; }
        }

    }
}