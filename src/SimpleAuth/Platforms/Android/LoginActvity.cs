//
//  Copyright 2016  Clancey
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Android.App;
using Android.Net.Http;
using Android.Webkit;
using Android.OS;
using System.Threading.Tasks;
using SimpleAuth.Droid;
using Android.Widget;


namespace SimpleAuth
{

	[Activity (Label = "Web Authenticator")]
	public class LoginActivity : Activity
	{
		public static string UserAgent = "";

		internal class State : Java.Lang.Object
		{
			public Authenticator Authenticator;
		}
		internal static readonly ActivityStateRepository<State> StateRepo = new ActivityStateRepository<State> ();

		State state;
		EditText username;
		EditText password;
		Button login;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			//
			// Load the state either from a configuration change or from the intent.
			//
			state = LastNonConfigurationInstance as State;
			if (state == null && Intent.HasExtra ("StateKey")) {
				var stateKey = Intent.GetStringExtra ("StateKey");
				state = StateRepo.Remove (stateKey);
			}
			if (state == null) {
				Finish ();
				return;
			}

			Title = state.Authenticator.Title;
			//
			// Build the UI
			//

			SetContentView (Resource.Layout.login);
			username = FindViewById<EditText> (Resource.Id.username);
			password = FindViewById<EditText> (Resource.Id.password);
			login = FindViewById<Button> (Resource.Id.loginButton);
			login.Click += async (s, e) => {
				var authenticator = state.Authenticator as IBasicAuthenicator;
				bool success = await authenticator.VerifyCredentials (username.Text, password.Text);

				if (success)
					Finish ();
			};

		}


		public override void OnBackPressed ()
		{
			Finish ();
			state.Authenticator.OnCancelled ();
		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			return state;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
		}

	}
}

