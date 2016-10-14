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

using Xamarin.Forms;
using SimpleAuth;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Sample.Forms
{
	public partial class LoginPage : ContentPage
	{
		Authenticator authenticator;
		public LoginPage (Authenticator authenticator)
		{
			this.authenticator = authenticator;
			InitializeComponent ();
		}

		async void Handle_Clicked (object sender, System.EventArgs e)
		{
			if (string.IsNullOrWhiteSpace (Username.Text)) {
				await this.DisplayAlert ("Error", "Username is invalid", "Ok");
				return;
			}

			if (string.IsNullOrWhiteSpace (Password.Text)) {
				await this.DisplayAlert ("Error", "Password is invalid", "Ok");
				return;
			}

			try {
				var oauth = authenticator as OAuthPasswordAuthenticator;
				bool success = false;
				if (oauth != null) {
					success = await oauth.VerifyCredentials (Username.Text, Password.Text);
				}

				var basic = authenticator as BasicAuthAuthenticator;
				if (basic != null) {
					success = await basic.CheckCredentails (Username.Text, Password.Text);
				}

				if (success)
					await this.Navigation.PopModalAsync ();
				else
					await this.DisplayAlert ("Error", "Invalid credentials", "Ok");
				
			} catch (Exception ex) {
				await this.DisplayAlert ("Error", ex.Message, "Ok");
			}
		}

	}
}
