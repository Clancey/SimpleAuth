Simple Auth
================
Every API needs authentication, yet no developer wants to deal with authentication. Simple Auth embeds authentication into the API so you dont need to deal with it. Most importantly it works great with traditional Xamarin and Xamarin.Forms
[![Build status](https://ci.appveyor.com/api/projects/status/ldi3o0g14p0ugljq?svg=true)](https://ci.appveyor.com/project/Clancey/simpleauth)

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

On password support is for iOS Only.  
Simply add the project or the Nuget

https://www.nuget.org/packages/Clancey.SimpleAuth.OnePassword/

Then call the following line in your iOS project prior to calling api.Authenticate();
```cs
SimpleAuth.OnePassword.Activate();
```
