using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuth;
using SimpleAuth.Providers;
using Xamarin.Forms;

namespace Sample.Forms
{
	public class App : Application
	{

		AuthenticatedApi api;
		public App ()
		{

			//Hook up our Forms login page for Basic Auth
			BasicAuthApi.ShowAuthenticator = (BasicAuthAuthenticator obj) => {
					MainPage.Navigation.PushModalAsync (new LoginPage (obj));
			};

			var scopes = new[]
			{
				"https://www.googleapis.com/auth/userinfo.email",
				"https://www.googleapis.com/auth/userinfo.profile"
			};
			//api = new GoogleApi("google",
			//	   "clientid",
			//	"clientsecret",new ModernHttpClient.NativeMessageHandler())
			//{
			//	Scopes = scopes,
			//};

			//api = new FacebookApi("facebook","","");
			api = new OAuthPasswordApi ("myapi", "clientid", "clientsecret",
										"https://serverurl.com",
										"https://tokenurl.com",
										"https://refreshurl.com") {
				//Hook up our Forms login page for Oauth with Password
				CurrentShowAuthenticator = (obj) => {
					MainPage.Navigation.PushModalAsync (new LoginPage (obj));
				},
			};

			var button = new Button
			{
				Text = "Login",
			};
			button.Clicked += async (sender, args) =>
			{
				try
				{
					var account = await api.Authenticate();
					var me = await api.Get("me");

					Console.WriteLine(account.Identifier);
				}
				catch (TaskCanceledException)
				{
					Console.WriteLine("Canceled");
				}
			};
			// The root page of your application
			MainPage = new NavigationPage(new ContentPage {
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
						button
					}
				}
			});
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
