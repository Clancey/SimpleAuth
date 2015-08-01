using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuth;
using SimpleAuth.OAuth;
using SimpleAuth.Providers;
using Xamarin.Forms;

namespace Sample.Forms
{
	public class App : Application
	{

		OAuthApi api;
		public App ()
		{
			var scopes = new[]
			{
				"https://www.googleapis.com/auth/userinfo.email",
				"https://www.googleapis.com/auth/userinfo.profile"
			};
			api = new GoogleApi("google",
				   "clientid",
				   "clientsecret")
			{
				Scopes = scopes,
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
