using AppKit;
using Foundation;

namespace Sample.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;


		public AppDelegate()
		{
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			mainWindowController = new MainWindowController();
			mainWindowController.Window.MakeKeyAndOrderFront(this);
			NSAppleEventManager.SharedAppleEventManager.SetEventHandler (this, new ObjCRuntime.Selector ("UrlHandleEvent:event:replyEvent"), AEEventClass.Internet, AEEventID.GetUrl);
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}

		[Export("UrlHandleEvent:event:replyEvent")]
		public void UrlHandleEvent (NSAppleEventDescriptor evt, NSAppleEventDescriptor replyEvt)
		{
			var url = evt.ParamDescriptorForKeyword (AppleEventParameters.DirectObject).StringValue;
			SimpleAuth.NativeSafariAuthenticator.ResumeAuth (url);
		}

	}
}
