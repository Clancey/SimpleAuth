using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;

namespace Sample.Forms.Droid
{
	[Activity (Label = "Sample.Forms", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SimpleAuth.Providers.Google.Init (this.Application);
			global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new Sample.Forms.App ());
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			SimpleAuth.Native.OnActivityResult (requestCode, resultCode, data);
		}
	}
}

