using System;

using Foundation;
using AppKit;
using SimpleAuth.Providers;
using SimpleAuth;
using System.Threading.Tasks;

namespace Sample.Mac
{
	public partial class MainWindowController : NSWindowController
	{

		GoogleApi api;
		public MainWindowController(IntPtr handle)
			: base(handle)
		{
		}

		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder)
			: base(coder)
		{
		}

		public MainWindowController()
			: base("MainWindow")
		{
		}
		public static string ClientId = "";
		public static string ClientSecret = "";


		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
			var scopes = new[]
			{
				"https://www.googleapis.com/auth/userinfo.email",
				"https://www.googleapis.com/auth/userinfo.profile"
			};
			api = new GoogleApi("google",
				ClientId,
				ClientSecret)
			{
				Scopes = scopes,
			};
		}

		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}

		partial void Login (Foundation.NSObject sender)
		{
			Login();

		}

		async void Login()
		{
			try
			{
				var account = await api.Authenticate();
				var info = await api.GetUserInfo();
				Console.WriteLine(account.Identifier);
			}
			catch (TaskCanceledException ex)
			{
				Console.WriteLine("Canceled");
			}
		}
	}
}
