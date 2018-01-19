using System;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
namespace SimpleAuth
{
    
    internal class ActivityLifecycleCallbackManager : Java.Lang.Object, global::Android.App.Application.IActivityLifecycleCallbacks
    {
        public FragmentActivity CurrentActivity { get; private set; }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CurrentActivity = activity as FragmentActivity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
            CurrentActivity = null;
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CurrentActivity = activity as FragmentActivity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CurrentActivity = activity as FragmentActivity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}
