using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using SimpleAuth;
using SimpleAuth;

namespace Sample.Droid
{
	[Activity(Label = "Sample.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		OAuthApi api;
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
			api = new OAuthApi("google",new OAuthAuthenticator(
				"authUrl",
				"tokenUrl",
				"redirecturl",
				"clientid",
				"clientsecret"));
			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.MyButton);
			button.Text = "Login";
			button.Click += async delegate
			{
				var account = await api.Authenticate();
				Console.WriteLine(account.Identifier);
			};
		}
	}
}

