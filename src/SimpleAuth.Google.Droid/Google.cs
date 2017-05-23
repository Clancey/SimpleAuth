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
using Android.OS;

namespace SimpleAuth.Providers
{
    public class Google
    {
        static ActivityLifecycleManager activityLifecycle = new ActivityLifecycleManager();

        public static void Init(global::Android.App.Application app)
        {
            app.RegisterActivityLifecycleCallbacks(activityLifecycle);

			Native.RegisterCallBack ("google", OnActivityResult);
			GoogleApi.IsUsingNative = true;
            GoogleApi.GoogleShowAuthenticator = Login;
			GoogleApi.OnLogOut = LogOut ;
        }

        public static void Uninit(global::Android.App.Application app)
        {
            app.UnregisterActivityLifecycleCallbacks(activityLifecycle);
        }

		static async void LogOut (string clientId, string clientsecret)
		{
			googleSignInProvider?.Canceled ();
			googleSignInProvider = new GoogleSignInProvider ();
			await googleSignInProvider.SignOut (clientId);
		}
		static GoogleSignInProvider googleSignInProvider;
        static async void Login(WebAuthenticator authenticator)
        {
            var currentActivity = activityLifecycle.CurrentActivity;

            var googleAuth = authenticator as GoogleAuthenticator;
			googleSignInProvider?.Canceled ();
            googleSignInProvider = new GoogleSignInProvider();

            GoogleSignInResult result = null;
            try
            {
                result = await googleSignInProvider.Authenticate(googleAuth);

                if (result == null || result.Status.IsCanceled || result.Status.IsInterrupted)
                {
                    googleAuth.OnCancelled();
                    return;
                }

                if (!result.IsSuccess)
                {
					//This is a cursed error. This meands something is wrong with the tokens. Easiest to regenerate new Web ones
					if (result.Status.StatusCode == 12501) {
						googleAuth.OnError ("Your tokens/signing is bad. Go regenerate new OAuth/Web application tokens");
					}
					googleAuth.OnError(result.Status.StatusMessage ?? result.Status.ToString ());
                    return;
                }

				var gauth = authenticator as GoogleAuthenticator;
				gauth.IdToken = result?.SignInAccount?.IdToken;
				gauth.ServerToken = result?.SignInAccount?.ServerAuthCode;
				string accessToken;
				//Going to use standard OAuth Flow since we have a Secret
				if (googleAuth.ClientSecret != GoogleApi.NativeClientSecret) {
					accessToken = result.SignInAccount.ServerAuthCode;
				} else {
					if (result?.SignInAccount?.Account == null) {
						accessToken = result.SignInAccount.IdToken;

					}
					else {
						//Just rely on the native lib for refresh
						var tokenScopes = googleAuth.Scope.Select (s => "oauth2:" + s);
						accessToken = await Task.Run (() => {
							return Android.Gms.Auth.GoogleAuthUtil.GetToken (currentActivity, result?.SignInAccount?.Account, string.Join (" ", tokenScopes));
						});
					}
				}

				googleAuth.OnRecievedAuthCode (accessToken);

            }
            catch (Exception ex)
            {
                googleAuth.OnError(ex.Message);
            }

        }

        internal static Activity CurrentActivity
        {
            get {
				var activity = activityLifecycle?.CurrentActivity;

				if (activity == null)
					throw new NullReferenceException ("Current Activity is not set.  Make sure you call the Platform specific Init() method.");

				return activity;
			}
        }

        public static bool OnActivityResult(int requestCode, Result result, Intent data)
        {
			// Result returned from launching the Intent from GoogleSignInApi.getSignInIntent(...);
			if (requestCode == GoogleSignInProvider.SIGN_IN_REQUEST_CODE) {
				var googleSignInResult = Auth.GoogleSignInApi.GetSignInResultFromIntent (data);

				googleSignInProvider?.FoundResult (googleSignInResult);
				return true;
			} 
			//else if (result == Result.Canceled) {
			//	googleSignInProvider.Canceled();
			//}
			return false;
        }


		internal class GoogleSignInProvider : Java.Lang.Object, GoogleApiClient.IOnConnectionFailedListener, GoogleApiClient.IConnectionCallbacks
        {
            internal const int SIGN_IN_REQUEST_CODE = 41221;

            GoogleApiClient googleApiClient;

            TaskCompletionSource<GoogleSignInResult> tcsSignIn;
            public void FoundResult(GoogleSignInResult result)
            {
                if (tcsSignIn != null && !tcsSignIn.Task.IsCompleted)
                    tcsSignIn.TrySetResult(result);
            }

            public async Task<GoogleSignInResult> Authenticate(GoogleAuthenticator authenticator)
            {

				var activity = CurrentActivity;
				var availabilityApi = GoogleApiAvailability.Instance;
				var isAvailable = availabilityApi.IsGooglePlayServicesAvailable (activity);
				if (isAvailable != ConnectionResult.Success) {
					if (availabilityApi.IsUserResolvableError (isAvailable)) {
						availabilityApi.GetErrorDialog (activity, isAvailable, SIGN_IN_REQUEST_CODE);
						throw new Exception (availabilityApi.GetErrorString (isAvailable));
					} else {
						throw new Exception ("This device is not Supported");
					}
				}
				try {
					var googleScopes = authenticator.Scope?.Select (s => new Scope (s))?.ToArray ();

					var gsoBuilder = new GoogleSignInOptions.Builder (GoogleSignInOptions.DefaultSignIn)
					                                        .RequestIdToken (GoogleAuthenticator.GetGoogleClientId (authenticator.ClientId))
					                                        .RequestServerAuthCode (GoogleAuthenticator.GetGoogleClientId (GoogleApi.CleanseClientId (authenticator.ServerClientId)) ?? authenticator.ClientId);
					//.RequestEmail ();

					var gso = gsoBuilder.Build ();

					googleApiClient = new GoogleApiClient.Builder (activity)
			                                 .AddConnectionCallbacks (this)
			                                 .AddOnConnectionFailedListener (this)
											 .AddApi (Auth.GOOGLE_SIGN_IN_API, gso)
					                                           .Build ();
					googleApiClient.Connect ();

					var signInIntent = Auth.GoogleSignInApi.GetSignInIntent (googleApiClient);

					if (tcsSignIn != null && !tcsSignIn.Task.IsCompleted)
						tcsSignIn.TrySetCanceled ();

					tcsSignIn = new TaskCompletionSource<GoogleSignInResult> ();

					activity.StartActivityForResult (signInIntent, SIGN_IN_REQUEST_CODE);

					var success = await tcsSignIn.Task;
					return success;
				} finally {
					googleApiClient?.UnregisterConnectionCallbacks (this);
					googleApiClient?.Disconnect ();
				}
            }
			TaskCompletionSource<bool> connectedTask = new TaskCompletionSource<bool> ();
			public async Task<bool> SignOut (string serverClientId)
			{
				try {
					var gsoBuilder = new GoogleSignInOptions.Builder (GoogleSignInOptions.DefaultSignIn)
																.RequestIdToken (serverClientId)
																.RequestServerAuthCode (serverClientId)
																.RequestEmail ();

					var gso = gsoBuilder.Build ();

					googleApiClient = new GoogleApiClient.Builder (CurrentActivity)
											 .AddConnectionCallbacks (this)
											 .AddOnConnectionFailedListener (this)
											 .AddApi (Auth.GOOGLE_SIGN_IN_API, gso)
															   .Build ();
					googleApiClient.Connect ();
					await connectedTask.Task;
					var result = await Auth.GoogleSignInApi.SignOut (googleApiClient).AsAsync<Statuses> ();
					return true;
				} finally {
					googleApiClient?.UnregisterConnectionCallbacks (this);
					googleApiClient?.Disconnect ();
				}
				//var result = await Auth.GoogleSignInApi.SignOut (googleApiClient).AsAsync<ResultS> ();
				//result.
			}

            public void OnConnectionFailed(ConnectionResult result)
            {
				connectedTask?.TrySetResult (false);
				googleApiClient?.Disconnect ();
				var message = GoogleApiAvailability.Instance.GetErrorDialog (CurrentActivity,result.ErrorCode,SIGN_IN_REQUEST_CODE);
				message.Show ();
				Console.WriteLine (message);
				tcsSignIn?.TrySetException (new Exception (result.ErrorMessage));
            }

			public void Canceled ()
			{
				tcsSignIn?.TrySetCanceled ();
			}

			public void OnConnected (Bundle connectionHint)
			{
				connectedTask?.TrySetResult (true);
				Console.WriteLine ("Connected");
			}

			public void OnConnectionSuspended (int cause)
			{
				googleApiClient?.Connect ();
			}
		}
    }
}
