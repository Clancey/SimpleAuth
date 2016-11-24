using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace SimpleAuth.Providers
{
    class ActivityLifecycleManager : Java.Lang.Object, global::Android.App.Application.IActivityLifecycleCallbacks
    {
        public Activity CurrentActivity { get; private set; } = null;

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CurrentActivity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}
