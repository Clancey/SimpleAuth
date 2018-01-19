using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SimpleAuth.Droid.CustomTabs
{
    public class SimpleAuthCallbackActivity : Activity
    {
        protected override void OnCreate(Android.OS.Bundle bundle)
        {
            base.OnCreate(bundle);
            Native.OnActivityResult(0, Result.Ok, this.Intent);
            Finish();
        }
    }
}
