using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Android.Runtime;
using Android.Gms.Extensions;
using Android.Gms.Auth.Api;
using Android.Content;
using Android.Gms.Common;

[assembly: UsesPermission (Android.Manifest.Permission.GetAccounts)]

namespace SimpleAuth.Providers
{
    public class Google
    {
        static ActivityLifecycleManager activityLifecycle = new ActivityLifecycleManager();

        public static void Init(global::Android.App.Application app)
        {
            app.RegisterActivityLifecycleCallbacks(activityLifecycle);

            GoogleApi.IsUsingNative = true;
            GoogleApi.ShowAuthenticator = Login;
        }

        public static void Uninit(global::Android.App.Application app)
        {
            app.UnregisterActivityLifecycleCallbacks(activityLifecycle);
        }

        static GoogleSignInProvider googleSignInProvider = new GoogleSignInProvider();

        static async void Login(WebAuthenticator authenticator)
        {
            var currentActivity = activityLifecycle.CurrentActivity;

            var googleAuth = authenticator as GoogleAuthenticator;

            googleSignInProvider = new GoogleSignInProvider();

            GoogleSignInResult result = null;

            try
            {
                result = await googleSignInProvider.Authenticate(GoogleAuthenticator.GetGoogleClientId (googleAuth.ClientId), googleAuth.Scope.ToArray ());

                if (result == null || result.Status.IsCanceled || result.Status.IsInterrupted)
                {
                    googleAuth.OnCancelled();
                    return;
                }

                if (!result.IsSuccess)
                {
                    googleAuth.OnError(result.Status.StatusMessage);
                    return;
                }

                //var accessToken = Android.Gms.Auth.GoogleAuthUtil.GetToken(currentActivity, result.SignInAccount.Email, string.Join (" ", tokenScopes));


                var androidAccount = Android.Accounts.AccountManager.FromContext(currentActivity)
                                            ?.GetAccounts()
                                            ?.FirstOrDefault(a => a.Name?.Equals(result?.SignInAccount?.Email, StringComparison.InvariantCultureIgnoreCase) ?? false);

                var tokenScopes = googleAuth.Scope.Select(s => "oauth2:" + s);

                var accessToken = await Task.Run(() =>
                {
                    return Android.Gms.Auth.GoogleAuthUtil.GetToken(currentActivity, androidAccount, string.Join (" ", tokenScopes));
                });

                googleAuth.OnRecievedAuthCode (accessToken);

            }
            catch (Exception ex)
            {
                googleAuth.OnError(ex.Message);
            }

        }

        internal static global::Android.Support.V4.App.FragmentActivity CurrentActivity
        {
            get
            {
                var activity = activityLifecycle?.CurrentActivity;

                if (activity == null)
                    throw new NullReferenceException("Current Activity is not set.  Make sure you call the Platform specific Init() method.");
                
                if (!(activity is Android.Support.V4.App.FragmentActivity))
                    throw new InvalidCastException("You must use call Google Social Auth from an Activity derived from Android.Support.V4.App.FragmentActivity!");

                return activity.JavaCast<Android.Support.V4.App.FragmentActivity>();
            }
        }

        public static void OnActivityResult(int requestCode, Result result, Intent data)
        {
            // Result returned from launching the Intent from GoogleSignInApi.getSignInIntent(...);
            if (requestCode == GoogleSignInProvider.SIGN_IN_REQUEST_CODE)
            {
                var googleSignInResult = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);

                googleSignInProvider?.FoundResult(googleSignInResult);
            }
        }


        internal class GoogleSignInProvider : Java.Lang.Object, GoogleApiClient.IOnConnectionFailedListener
        {
            internal const int SIGN_IN_REQUEST_CODE = 41221;

            GoogleApiClient googleApiClient;

            TaskCompletionSource<GoogleSignInResult> tcsSignIn;

            public void FoundResult(GoogleSignInResult result)
            {
                if (tcsSignIn != null && !tcsSignIn.Task.IsCompleted)
                    tcsSignIn.TrySetResult(result);
            }

            public async Task<GoogleSignInResult> Authenticate(string serverClientId, params string[] scopes)
            {
                var googleScopes = scopes?.Select(s => new Scope(s))?.ToArray();

                var gsoBuilder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                                                        .RequestIdToken(serverClientId)
                                                        .RequestEmail ()
                                                        ;

                //if (googleScopes != null && googleScopes.Any())
                  //  gsoBuilder = gsoBuilder.RequestScopes(googleScopes.First(), googleScopes.Skip(1)?.ToArray());


                var gso = gsoBuilder.Build();

                var activity = CurrentActivity;

                googleApiClient = await new GoogleApiClient.Builder(activity)
                                         .EnableAutoManage(activity, this)
                                         .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                                         .BuildAndConnectAsync();

                var signInIntent = Auth.GoogleSignInApi.GetSignInIntent(googleApiClient);

                if (tcsSignIn != null && !tcsSignIn.Task.IsCompleted)
                    tcsSignIn.TrySetCanceled();

                tcsSignIn = new TaskCompletionSource<GoogleSignInResult>();

                activity.StartActivityForResult(signInIntent, SIGN_IN_REQUEST_CODE);

                return await tcsSignIn.Task;
            }

            public void OnConnectionFailed(ConnectionResult result)
            {
                // TODO: Raise failure
            }
        }
    }
}
