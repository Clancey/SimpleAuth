using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuth;
using SimpleAuth.OAuth;
using Xamarin.Forms;

namespace Sample.Forms
{
	public class App : Application
	{

		OAuthApi api;
		public App ()
		{
			api = new OAuthApi("google", new OAuthAuthenticator(
				   "https://accounts.google.com/o/oauth2/auth", //Auth Url
				   "https://accounts.google.com/o/oauth2/token", //Token Url
				   "http://localhost",
				   "clientid",
				   "clientsecret"));
			var button = new Button
			{
				Text = "Login",
			};
			button.Clicked += async (sender, args) =>
			{
				try
				{
					var account = await api.Authenticate();
					Console.WriteLine(account.Identifier);
				}
				catch (TaskCanceledException ex)
				{
					Console.WriteLine("Canceled");
				}
			};
			// The root page of your application
			MainPage = new ContentPage {
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
						button
					}
				}
			};
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
