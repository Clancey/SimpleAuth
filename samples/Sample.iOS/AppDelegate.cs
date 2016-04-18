using Foundation;
using UIKit;
using MonoTouch.Dialog;
using System;
using SimpleAuth.Providers;
using System.Threading.Tasks;
using SimpleAuth;

namespace Sample.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window
		{
			get;
			set;
		}
		GoogleApi googleApi = new GoogleApi("google","clientid","clientsecret");
		ApiKeyApi apiKeyApi = new ApiKeyApi ("myapikey", "api_key", AuthLocation.Query){
			BaseAddress = new Uri("http://petstore.swagger.io/v2"),
		};
		BasicAuthApi basicApi = new BasicAuthApi ("github","https://api.github.com"){UserAgent = "SimpleAuthDemo"};
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			SimpleAuth.OnePassword.Activate ();
			Api.UnhandledException += (sender, e) =>
			{
				Console.WriteLine(e);
			};
			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			Window.RootViewController = new DialogViewController (new RootElement ("Simple Auth") {
				new Section("Google Api"){
					new StringElement("Authenticate", () => {
						RunWithSpinner("Authenticating",async ()=>{
							var account = await googleApi.Authenticate();
							ShowAlert("Success","Authenticate");
						});
					}),
					new StringElement("Log out", async () => {
						googleApi.ResetData();
						ShowAlert ("Success", "Logged out");
					}),
				},
				new Section("Api Key Api")
				{
					new StringElement("Get", async () => await RunWithSpinner ("Querying", async () => {
						var account = await apiKeyApi.Get ("http://petstore.swagger.io/v2/store/inventory?test=test1");
						ShowAlert ("Success", "Querying");
					})),
				},
				new Section("Basic Auth"){
					new StringElement("Login to Github", async () => {
						var account = await basicApi.Authenticate();
						ShowAlert ("Success", "Authenticated");
					}),
					new StringElement("Log out", async () => {
						basicApi.ResetData();
						ShowAlert ("Success", "Logged out");
					}),
				}
			});

			// make the window visible
			Window.MakeKeyAndVisible();

			return true;
		}

		async Task RunWithSpinner(string status,Func<Task> task)
		{
			try{
				using(new Spinner(status)){
					await task.Invoke();
					ShowAlert("Success","");
				}
			}
			catch(TaskCanceledException){
				ShowAlert("Canceled","");
			}
			catch(Exception ex)
			{
				ShowAlert("Error",ex.ToString());
			}
		}
		void ShowAlert(string title, string message)
		{
			var alert = UIAlertController.Create (title, message, UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("Ok",UIAlertActionStyle.Default,(a)=>{

			}));
			Window.RootViewController.PresentViewControllerAsync (alert, true);

		}
		public override void OnResignActivation(UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate(UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}
	}
}


