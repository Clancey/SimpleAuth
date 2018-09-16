# Simple Auth

Every API needs authentication, yet no developer wants to deal with authentication. Simple Auth embeds authentication into the API so you dont need to deal with it. Most importantly it works great with traditional Xamarin and Xamarin.Forms

Android: [![Android Build status](https://build.appcenter.ms/v0.1/apps/7e5acfa7-b0fb-4468-8e0a-f947abcded95/branches/master/badge)](https://appcenter.ms)

iOS/MacOS: [![Build status](https://build.appcenter.ms/v0.1/apps/fabfc2aa-5ed3-420b-b257-343b158c176e/branches/master/badge)](https://appcenter.ms)


# General information


## Available on Nuget

[Clancey.SimpleAuth](https://www.nuget.org/packages/Clancey.SimpleAuth/)

## Providers

### Current Built in Providers

* Azure Active Directory
* Amazon
* Dropbox
* Facebook
* Github
* Google
* Instagram
* Linked In
* Microsoft Live Connect
* Twitter

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


## Restful Api Requests

Restful Api Requests couldnt be simpler

```cs
var song = await api.Get<Song>("http://myapi/Song/",songId);
```

Paramaters can be added as part of the path

```cs
var todoItem = await api.Get<TodoItem>("http://myapi/user/{UserId}/TodoItem",new Dictionary<string,string>{["UserId"] = "1", ["itemID"] = "22"});
```
Generates the following Url:

```cs
http://myapi/user/1/TodoItem?itemID=22
```


## Attribute your Api Requests (Optional)

```cs
[Path("/pet")]
[ContentType("application/json")]
[Accepts("application/json")]
public virtual Task AddPet(Pet body) {
    return Post( body);
}
```

## Webview Authentication

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

# iOS/Mac Specific

## OnePassword Support (iOS)

One password support is for iOS Only.  
Simply add the project or the Nuget

[Clancey.SimpleAuth.OnePassword](https://www.nuget.org/packages/Clancey.SimpleAuth.OnePassword/)

Then call the following line in your iOS project prior to calling api.Authenticate();
```cs
SimpleAuth.OnePassword.Activate();
```

## Native Twitter Support via Twitter App
You can use the Twitter app to authenticate with SimpleAuth on iOS. 

Add the following to your Info.Plist
```
// Info.plist
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>twitterkit-<consumerKey></string>
    </array>
  </dict>
</array>
<key>LSApplicationQueriesSchemes</key>
<array>
    <string>twitter</string>
    <string>twitterauth</string>
</array>
```

Then call the following line in your iOS AppDelegate FinishedLaunching method;

```cs
SimpleAuth.Providers.Twitter.Init();
```

Also add the following override in your AppDelegate

```cs
public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
{
	if (SimpleAuth.Native.OpenUrl(app, url, options))
		return true;
	return base.OpenUrl(app,url,options);
}
```

## Native Facebook Support via iOS SDK 
  
Simply add the project or the Nuget

[Clancey.SimpleAuth.Facebook.iOS](https://www.nuget.org/packages/Clancey.SimpleAuth.Facebook.iOS/)

The Facebook SDK requires you modify your info.plist : https://components.xamarin.com/gettingstarted/facebookios

Then call the following line in your iOS AppDelegate FinishedLaunching method;

```cs
SimpleAuth.Providers.Facebook.Init(app, options);
```

Also add the following override in your AppDelegate

```cs
public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
{
	if (SimpleAuth.Native.OpenUrl(app, url, options))
		return true;
	return base.OpenUrl(app,url,options);
}
```

## Native Google Support via iOS SDK

[Clancey.SimpleAuth.Facebook.iOS](https://www.nuget.org/packages/Clancey.SimpleAuth.Facebook.iOS/)

The Google SDK can do Cross-Client Login.  This allows you to get tokens for the server, with one login.

To use Cross-client you need to set the ServerClientId on the GoogleApi. 

Call the following in your FinishedLaunching Method;

```cs
SimpleAuth.Providers.Google.Init()
```

Also add the following to your AppDelegate


```cs
public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
{
	if (SimpleAuth.Native.OpenUrl(app, url, options))
		return true;
	return base.OpenUrl(app,url,options);
}
```

If you need Cross-client authentication

```cs
var api = new GoogleApi("google","client_id"){
	ServerClientId = "server_client_id""
};
var account = await api.Authenticate ();
var serverToken = account.UserData ["ServerToken"];
```

### Troubleshooting

```
System.Exception: Error Domain=com.google.GIDSignIn Code=-2 "keychain error" UserInfo={NSLocalizedDescription=keychain error}
```

Under the iOS Build Signing, Custom Entitlements: make sure an entitlement.plist is set 


## Native SFSafariViewController iOS/MacOS

SFSafariViewController Allows users to use Safari to login, instead of embedded webviews.

Google now requires this mode and is enabled by default for Google Authentication on iOS/MacOS.

Then call the following line in your iOS AppDelegate FinishedLaunching method;

```cs
SimpleAuth.NativeSafariAuthenticator.Activate ();
```

To use the Native Safari Authenticator, you are required to add the following snippet in your AppDelegate (**iOS Only**)

```cs
public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
{
	if (SimpleAuth.Native.OpenUrl(app, url, options))
		return true;
	return base.OpenUrl(app,url,options);
}

```

You are also required to add the following to add a CFBundleURLSchemes to your info.plist 

For Google: com.googleusercontent.apps.YOUR_CLIENT_ID

```
	<key>CFBundleURLTypes</key>
	<array>
		<dict>
			<key>CFBundleURLSchemes</key>
			<array>
				<string>com.googleusercontent.apps.YOURCLIENTID</string>
			</array>
			<key>CFBundleURLName</key>
			<string>googleLogin</string>
		</dict>
	</array>
	
```



# Android

## Google Sign-In on Android

Simple Auth supports the native Google Sign-in for Android.

1. Add the nuget 
[Clancey.SimpleAuth.Google.Droid](https://www.nuget.org/packages/Clancey.SimpleAuth.Google.Droid/)
2. Create OAuth Client Id (Web Application): [Link](https://console.developers.google.com/apis/credentials)
3. Create and OAuth Android app: [Link](https://console.developers.google.com/apis/credentials)
	* Sign your app using the same Keystore
4. Use both the Web Application ClientID. ClientSecret is not required but reccomended.
5. Add the following code to your Main Activity

	```cs
	protected override void OnCreate(Bundle bundle)
	{
		base.OnCreate(bundle);
		SimpleAuth.Providers.Google.Init(this.Application);
		//The rest of your initialize code
	}
	
	protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
	{
	   base.OnActivityResult(requestCode, resultCode, data);
		SimpleAuth.Native.OnActivityResult (requestCode,resultCode,data); 
	}
	```

If you need Cross-Client authentication pass your ServerClientId into the google api

```cs
var api = new GoogleApi("google","client_id"){
	ServerClientId = "server_client_id""
};
var account = await api.Authenticate ();
var serverToken = account.UserData ["ServerToken"];
```


### Troubleshooting
If you get:
 ```Unable to find explicit activity class {com.google.android.gms.auth.api.signin.internal.SignInHubActivity}; have you declared this activity in your AndroidManifest.xml?```

Add the following to your AndroidManifest.xml

```
<activity android:name="com.google.android.gms.auth.api.signin.internal.SignInHubActivity"
		android:screenOrientation="portrait"
		android:windowSoftInputMode="stateAlwaysHidden|adjustPan" />
	</application>
```
### Status Code 12501 (unknown) Your app signing or tokens are invalid
1. Check your app is signed with the same KeyStore noted in for your android app [Link](https://console.developers.google.com/apis/credentials)
2. Regenerate new OAuth 2 Client id, create the WebApplication kind.

## Native Facebook for Android

Simple Auth supports the native Facebook SDK for Android.

1. Add the nuget 
[Clancey.SimpleAuth.Facebook.Droid](https://www.nuget.org/packages/Clancey.SimpleAuth.Facebook.Droid/)
2. Create an Android App: [Link](https://developers.facebook.com/docs/facebook-login/android)
3. Add the following to your String.xml in Resources/values. If your appId was 1066763793431980
	```
	<string name="facebook_app_id">1066763793431980</string>
	<string name="fb_login_protocol_scheme">fb1066763793431980</string>
	```
4. Add a meta-data element to the application element: 
	```
	[assembly: MetaData("com.facebook.sdk.ApplicationId", Value = "@string/facebook_app_id")]
	```
5. 	Add FacebookActivity to your AndroidManifest.xml:
	```
	<activity android:name="com.facebook.FacebookActivity"
          android:configChanges=
                 "keyboard|keyboardHidden|screenLayout|screenSize|orientation"
          android:label="@string/app_name" />          
	<activity
	    android:name="com.facebook.CustomTabActivity"
	    android:exported="true">
	    <intent-filter>
	        <action android:name="android.intent.action.VIEW" />
	        <category android:name="android.intent.category.DEFAULT" />
	        <category android:name="android.intent.category.BROWSABLE" />
	        <data android:scheme="@string/fb_login_protocol_scheme" />
	    </intent-filter>
	</activity>
	```
6. Add the following code to your Main Activity

	```cs
	protected override void OnCreate(Bundle bundle)
	{
		base.OnCreate(bundle);
		SimpleAuth.Providers.Google.Init(this.Application);
		//The rest of your initialize code
	}
	
	protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
	{
	   base.OnActivityResult(requestCode, resultCode, data);
		Native.OnActivityResult (requestCode,resultCode,data); 
	}
	```

## CustomTabs for Android

SimpleAuth supports using Custom Tabs for authorization.

1. Add the nuget [Clancey.SimpleAuth.Droid.CustomTabs](https://www.nuget.org/packages/Clancey.SimpleAuth.Droid.CustomTabs)
2. Create a subclass of SimpleAuthCallbackActivity to handle your url scheme, replacing the value of DataScheme with the scheme you used for the redirectUrl parameter of the Api constructor 

	```cs
	/// <summary>
    	/// Receives the redirect to the url scheme when authing
    	/// </summary>
    	[Activity(NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    	[IntentFilter(new [] { Intent.ActionView},
        	Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable},
        	DataScheme = "YOUR CUSTOM SCHEME")]
    	public class MyCallbackActivity : SimpleAuthCallbackActivity
    	{
    	}
	```
