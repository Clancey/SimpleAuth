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
        GoogleApi _googleApi;
        public App ()
		{

			//Hook up our Forms login page for Basic Auth
			BasicAuthApi.ShowAuthenticator = (IBasicAuthenicator obj) => {
				MainPage.Navigation.PushModalAsync (new LoginPage (obj));
			};

#if __ANDROID__
			string GoogleClientId = "646679266669-quj4b4frm8gi4knn6269me7ss9cefj7c.apps.googleusercontent.com";
			string GoogleSecret = GoogleApi.NativeClientSecret; //"uzj06SA8A66Y9mOA1rSjmQH7";
#else
			string GoogleClientId = "992461286651-k3tsbcreniknqptanrugsetiimt0lkvo.apps.googleusercontent.com";
			string GoogleSecret = "avrYAIxweNZwcHpsBlIzTp04";
#endif
            _googleApi = new GoogleApi("google", GoogleClientId, GoogleSecret)
            {
                Scopes = new[]
                                {
                                "https://www.googleapis.com/auth/userinfo.email",
                                "https://www.googleapis.com/auth/userinfo.profile"
                                },
            };

            var logoutButton = new Button()
            {
                Text = "Logout"
            };
            logoutButton.Clicked += (sender, args) =>
            {
                if (_googleApi != null)
                {
                    _googleApi.Logout();
                }
            };


            // The root page of your application
            MainPage = new NavigationPage(new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        CreateApiButton(_googleApi),
                        //CreateApiButton( new GoogleApi("google", GoogleClientId,GoogleSecret)
                        //{
                        //    Scopes =  new[]
                        //        {
                        //        "https://www.googleapis.com/auth/userinfo.email",
                        //        "https://www.googleapis.com/auth/userinfo.profile"
                        //        },
                        //}),
                        CreateApiButton(new FacebookApi("facebook","","")),
                        CreateApiButton(new OAuthPasswordApi ("myapi", "clientid", "clientsecret",
                                        "https://serverurl.com",
                                        "https://tokenurl.com",
                                        "https://refreshurl.com")),
                        logoutButton
                    }
                }
            });
        }

        private void LogoutButton_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        Button CreateApiButton(AuthenticatedApi api)
        {
            var button = new Button
            {
                Text = $"Login: {api.GetType().Name}",
            };
            button.Clicked += async (sender, args) =>
            {
				try {
					var account = await api.Authenticate ();
					Console.WriteLine (account.Identifier);
					MainPage.DisplayAlert ("Success!", "User is logged in", "Ok");
				} catch (TaskCanceledException) {
					Console.WriteLine ("Canceled");
					MainPage.DisplayAlert ("Error", "User Canceled", "Ok");
				} catch (Exception ex) {
					Console.WriteLine (ex);
					MainPage.DisplayAlert ("Error", ex.Message,"Ok");
				}
            };
            return button;

        }
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
