using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace SimpleAuth
{
	class BasicAuthController
	{
		readonly IBasicAuthenicator authenticator;

		public BasicAuthController(IBasicAuthenicator authenticator)
		{
			this.authenticator = authenticator;
		}

		public async void Show()
		{
			await GetCredentials("Login");
		}
		public async Task<Tuple<string, string>> GetCredentials(string title, string details = "")
		{
			try
			{
				var current = GetCurrentViewController();
				if (current == null)
					throw new Exception("window.RootViewController is not set");
				var loginEntry = new LoginEntryAlert(title, details);
				var result = await loginEntry.GetCredentials(current);
				if (string.IsNullOrWhiteSpace(result.Item1) || string.IsNullOrWhiteSpace(result.Item2))
				{
					result = await GetCredentials(title, "Invalid Credentials");
				}
				try
				{
					bool success = await authenticator.VerifyCredentials (result.Item1, result.Item2);

					if (!success)
						throw new Exception("Invalid Credentials");
				}
				catch (Exception ex)
				{
					result = await GetCredentials(title, $"Error: {ex.Message}");
				}
				return result;
			}
			catch (TaskCanceledException)
			{
				authenticator.OnCancelled();
				return null;
			}
		}
		static UIViewController GetCurrentViewController()
		{
			var window = UIKit.UIApplication.SharedApplication.KeyWindow;
			var root = window.RootViewController;
			if (root == null) return null;
			var current = root;
			while (current.PresentedViewController != null)
			{
				current = current.PresentedViewController;
			}
			return current;
		}

		public class LoginEntryAlert : IDisposable
		{
			readonly string title;
			readonly string details;
			UIAlertController alertController;
			UIAlertView alertView;

			public LoginEntryAlert(string title, string details = "")
			{
				this.title = title;
				this.details = details;
				alertController = UIAlertController.Create(title, details, UIAlertControllerStyle.Alert);
				if (alertController != null)
					setupAlertController();
				else
				{
					setupAlertView();
				}
			}

			void setupAlertController()
			{
				UITextField usernameField = null;
				UITextField passwordField = null;
				var cancel = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (alert) => { tcs.TrySetCanceled(); });
				var ok = UIAlertAction.Create("Login", UIAlertActionStyle.Default,
					a => { tcs.TrySetResult(new Tuple<string, string>(usernameField.Text, passwordField.Text)); });

				alertController.AddTextField(field =>
				{
					field.Placeholder = "Username";
					usernameField = field;
				});
				alertController.AddTextField(field =>
				{
					field.Placeholder = "Password";
					field.SecureTextEntry = true;
					passwordField = field;
				});
				alertController.AddAction(ok);
				alertController.AddAction(cancel);
			}

			void setupAlertView()
			{
				alertView = new UIAlertView(title, details, null, "Cancel", "Login")
				{
					AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput
				};
				alertView.Clicked += AlertViewOnClicked;
			}

			void AlertViewOnClicked(object sender, UIButtonEventArgs e)
			{
				if (e.ButtonIndex == alertView.CancelButtonIndex)
				{
					tcs.TrySetCanceled();
				}
				else
				{
					tcs.TrySetResult(new Tuple<string, string>(alertView.GetTextField(0)?.Text, alertView.GetTextField(1)?.Text));
				}
				alertView.Clicked -= AlertViewOnClicked;
			}

			TaskCompletionSource<Tuple<string, string>> tcs = new TaskCompletionSource<Tuple<string, string>>();

			public Task<Tuple<string, string>> GetCredentials(UIViewController fromController)
			{
				if (alertController != null)
				{
					fromController.PresentViewControllerAsync(alertController, true);
				}
				else
				{
					alertView.Show();
				}
				return tcs.Task;
			}

			public void Dispose()
			{
				alertController?.Dispose();
				alertView?.DismissWithClickedButtonIndex(alertView.CancelButtonIndex, true);
				alertView?.Dispose();
				tcs?.TrySetCanceled();
				tcs = null;
			}
		}
	}
}
