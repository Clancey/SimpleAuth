using SimpleAuth;
using SimpleAuth.Providers;

namespace Sample.Maui;

public partial class App : Application
{
    AuthenticatedApi _sourceApi;
    public App()
    {

        //Hook up our Forms login page for Basic Auth
        BasicAuthApi.ShowAuthenticator = (IBasicAuthenicator obj) => {
            MainPage.Navigation.PushModalAsync(new LoginPage(obj));
        };

        string FacebookClientId = "528997441203195";
        string FacebookSecret = "db801dd6f6bc40702ebbd70ecdef4d4e";

        string InstagramClientId = "f2c421d810824581ac758861e56e5340";
        string InstagramSecret = "2465b93d4cc44355a4ccf32f3eca707a";

#if __ANDROID__
        string GoogleClientId = "646679266669-quj4b4frm8gi4knn6269me7ss9cefj7c.apps.googleusercontent.com";
        string GoogleSecret = GoogleApi.NativeClientSecret; //"uzj06SA8A66Y9mOA1rSjmQH7";
#else
            string GoogleClientId = "992461286651-k3tsbcreniknqptanrugsetiimt0lkvo.apps.googleusercontent.com";
			string GoogleSecret = "avrYAIxweNZwcHpsBlIzTp04";
#endif

        var logoutButton = new Button()
        {
            Text = "Logout"
        };
        logoutButton.Clicked += async (sender, args) =>
        {
            if (_sourceApi != null)
            {
                _sourceApi.Logout();
                _sourceApi = null;
            }
        };


        // The root page of your application
        MainPage = new NavigationPage(new ContentPage
        {
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Children = {
                        CreateApiButton( new GoogleApi("google", GoogleClientId, GoogleSecret)
                        {
                            Scopes =  new[]
                                {
                                "https://www.googleapis.com/auth/userinfo.email",
                                "https://www.googleapis.com/auth/userinfo.profile"
                                },
                        }),
                        CreateApiButton(new FacebookApi("facebook", FacebookClientId, FacebookSecret)),
                        CreateApiButton(new InstagramApi("instagram" ,InstagramClientId, InstagramSecret)),
                        CreateApiButton(new OAuthPasswordApi ("myapi", "clientid", "clientsecret",
                                        "https://serverurl.com",
                                        "https://tokenurl.com",
                                        "https://refreshurl.com")),
                        logoutButton
                    }
            }
        });
    }

    Button CreateApiButton(AuthenticatedApi api)
    {
        var button = new Button
        {
            Text = $"Login: {api.GetType().Name}",
        };
        button.Clicked += async (sender, args) =>
        {
            try
            {
                _sourceApi = api;
                var account = await api.Authenticate() as OAuthAccount;
                Console.WriteLine(account.Identifier + " : " + account.Token);
                await MainPage.DisplayAlert("Success!", "User is logged in", "Ok");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Canceled");
                await MainPage.DisplayAlert("Error", "User Canceled", "Ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await MainPage.DisplayAlert("Error", ex.Message, "Ok");
            }
        };
        return button;

    }
}

