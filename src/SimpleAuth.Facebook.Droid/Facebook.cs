using System;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace SimpleAuth.Providers
{
    public class Facebook
    {
        public static void Init(Android.App.Application app, bool requestPublishPermissions)
        {
            RequestPublishPermissions = requestPublishPermissions;

            app.RegisterActivityLifecycleCallbacks(activityLifecycleManager);

			Native.RegisterCallBack ("facebook",OnActivityResult);
            FacebookApi.IsUsingNative = true;
            FacebookApi.ShowFacebookAuthenticator = Login;
			FacebookApi.OnLogOut = Logout;
        }

        public static void Uninit(Android.App.Application app)
        {
            app.UnregisterActivityLifecycleCallbacks(activityLifecycleManager);
        }

        public static bool OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
			return callbackManager?.OnActivityResult (requestCode, (int)resultCode, data) ?? false;
        }

        static ActivityLifecycleManager activityLifecycleManager = new ActivityLifecycleManager ();

        public static bool RequestPublishPermissions { get; set; }

        static ICallbackManager callbackManager = null;

        public static async void Login(WebAuthenticator authenticator)
        {
            var fbAuthenticator = authenticator as FacebookAuthenticator;

            var currentActivity = activityLifecycleManager.CurrentActivity;

            FacebookSdk.SdkInitialize(currentActivity);

            callbackManager = CallbackManagerFactory.Create();
            var loginManager = LoginManager.Instance;
            var fbHandler = new FbCallbackHandler();

            loginManager.RegisterCallback(callbackManager, fbHandler);

            fbHandler.Reset();

            if (RequestPublishPermissions)
                loginManager.LogInWithPublishPermissions(currentActivity, fbAuthenticator.Scope);
            else
                loginManager.LogInWithReadPermissions(currentActivity, fbAuthenticator.Scope);

            LoginResult result = null;

            try
            {
                result = await fbHandler.Task;
            }
            catch (Exception ex)
            {
                fbAuthenticator.OnError(ex.Message);
                return;
            }

            if (result == null)
                fbAuthenticator.OnCancelled();
            
            DateTime? expires = null;
            long expiresIn = -1;
            if (result?.AccessToken.Expires != null)
            {
                expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(result.AccessToken.Expires.Time);
                expiresIn = (long)(expires.Value - DateTime.Now).TotalSeconds;
            }

            fbAuthenticator.OnRecievedAuthCode(result?.AccessToken.Token, expiresIn);
        }
		public static void Logout()
		{
			LoginManager.Instance.LogOut();
		}
    }

    class FbCallbackHandler : Java.Lang.Object, IFacebookCallback
    {
        TaskCompletionSource<LoginResult> tcs = new TaskCompletionSource<LoginResult>();

        public void Reset()
        {
            if (tcs != null && !tcs.Task.IsCompleted)
            {
                tcs.TrySetResult (null);
            }

            tcs = new TaskCompletionSource<LoginResult>();
        }

        public Task<LoginResult> Task
        {
            get
            {
                return tcs.Task;
            }
        }
        public void OnCancel()
        {
            tcs.TrySetResult(null);
        }

        public void OnError(FacebookException ex)
        {
            tcs.TrySetException(new System.Exception(ex.Message));
        }

        public void OnSuccess(Java.Lang.Object data)
        {
            tcs.TrySetResult(data.JavaCast<LoginResult>());
        }
    }
}
