Simple Auth
================
Every API needs authentication, yet no developer wants to deal with authentication. Simple Auth embeds authentication into the API so you dont need to deal with it. Most importantly it works great with traditional Xamarin and Xamarin.Forms


Available on Nuget
================

https://www.nuget.org/packages/Clancey.SimpleAuth/

Providers
================

Simple auth ships with some built in providers so you just need to add your keys and scopes.

```cs
var scopes = new[]
{
	"https://www.googleapis.com/auth/userinfo.email",
	"https://www.googleapis.com/auth/userinfo.profile"
};
var api = new GoogleApi("google",
	   "clientid",
	   "clientsecret")
{
	Scopes = scopes,
};

var account = await api.Authenticate();
```


Restful Api Requests
================

Restful Api Requests couldnt be simpler

```cs
var song = await api.Get<Song>("http://myapi/Song/",songId);
```


Attribute your Api Requests (Optional)
================
```cs
[Path("/pet")]
[ContentType("application/json")]
[Accepts("application/json")]
public virtual Task AddPet(Pet body) {
    return Post( body);
}
```

Webview Authentication
================

The webview is automatically displayed for you.  If you want to handle displaying it your self you can!

```cs
Api.ShowAuthenticator = (authenticator) =>
{
	var invoker = new Foundation.NSObject();
	invoker.BeginInvokeOnMainThread(() =>
	{
		var vc = new iOS.WebAuthenticator(authenticator);
		//TODO: Present View Controller
	});
};
```

OnePassword Support
=============

One password support is for iOS Only.  
Simply add the project or the Nuget

https://www.nuget.org/packages/Clancey.SimpleAuth.OnePassword/

Then call the following line in your iOS project prior to calling api.Authenticate();
```cs
SimpleAuth.OnePassword.Activate();
```


Native Facebook Support via iOS SDK
=============

Native Facebook support is for iOS Only.  
Simply add the project or the Nuget

https://www.nuget.org/packages/Clancey.SimpleAuth.Facebook.iOS/

The Facebook SDK requires you modify your info.plist : https://components.xamarin.com/gettingstarted/facebookios

Then call the following line in your iOS AppDelegate FinishedLaunching method;

```cs
SimpleAuth.Providers.Facebook.Init(app, options);
```

Also add the following override in your AppDelegate

```cs
public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
{
	if (SimpleAuth.Providers.Facebook.OpenUrl(application, url, sourceApplication, annotation))
		return true;
	return base.OpenUrl(application, url, sourceApplication, annotation);
}
```


Native SFSafariViewController iOS/MacOS
=============

SFSafariViewController Allows users to use Safari to login, instead of embedded webviews.

Google now requires this mode and is enabled by default for Google Authentication on iOS/MacOS.

To use the Native Safari Authenticator, you are required to add the following snippet in your AppDelegate

```cs
public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
{
	if (NativeSafariAuthenticator.ResumeAuth (url.AbsoluteString))
		return true;
	return false;
}

```

You are also required to add the following to add a CFBundleURLSchemes to your info.plist 
com.googleusercontent.apps.YOUR_CLIENT_ID
