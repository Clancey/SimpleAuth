﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using SimpleAuth;
using SimpleAuth;
using Android.Support.V4.App;
using System.Threading.Tasks;

[assembly: MetaData("com.facebook.sdk.ApplicationId", Value = "@string/facebook_app_id")]

namespace Sample.Droid
{
    // We need to give the activity an explicit name for our acvitity that we provide to Facebook when we setup the app
    // in the dev console, so it can callback to the right deep link.
    [Activity(Label = "Sample.Droid", MainLauncher = true, Icon = "@drawable/icon", Name = "com.simpleauth.sample.MainActivity")]
    public class MainActivity : FragmentActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Initialize our native providers
            SimpleAuth.Providers.Google.Init(this.Application);
            SimpleAuth.Providers.Facebook.Init(this.Application, false);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            FindViewById<Button>(Resource.Id.loginGoogleNative).Click += async (sender, e) =>
            {
                // Create an OAuth credential and use its clientId
                var clientId = "28116949007-uho7tcbsm17l8uq7t96fk8i9d6ftugnd"; // Provide without the '.apps.googleusercontent.com' suffix

                var google = new SimpleAuth.Providers.GoogleApi("google", clientId, "native") {
                    Scopes = new [] { "https://www.googleapis.com/auth/userinfo.profile" }
                };

                var account = await AuthAsync(google);

                // .. Do something with account
            };

            FindViewById<Button>(Resource.Id.loginFacebookNative).Click += async (sender, e) =>
            {
                // Use the App ID
                var clientId = "1475840585777919";

                var fb = new SimpleAuth.Providers.FacebookApi("facebook", clientId, "native");

                var account = await AuthAsync(fb);

                // .. Do something with account
            };

            FindViewById<Button>(Resource.Id.loginGeneric).Click += async (sender, e) =>
            {
                var oauth = new OAuthApi("someprovider", new OAuthAuthenticator(
                    "authUrl",
                    "tokenUrl",
                    "redirecturl",
                    "clientid",
                    "clientsecret"));
                    
                var account = await AuthAsync(oauth);

                // .. Do something with account
            };
        }

        async Task<Account> AuthAsync(OAuthApi api)
        {
            Account result = null;
            try
            {
                result = await api.Authenticate(;
                Toast.MakeText(this, "Successfully Authenticated!", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Toast.MakeText(this, "Authentication Faild!", ToastLength.Long).Show();
            }

            if (result != null)
                Console.WriteLine(await result.ToJsonAsync());

            return result;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            SimpleAuth.Providers.Facebook.OnActivityResult(requestCode, resultCode, data);

            SimpleAuth.Providers.Google.OnActivityResult(requestCode, resultCode, data);
        }
    }
}

