//
//  Copyright 2017  (c) James Clancey
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
using Foundation;
using System.Collections.Generic;
namespace SimpleAuth
{
	public static class Native
	{
		static CallBackHelper helper = new CallBackHelper ();
		public static bool UrlHandleEvent (NSAppleEventDescriptor evt, NSAppleEventDescriptor replyEvt)
		{
			foreach (var item in callBacks) {
				var s = item.Value (evt, replyEvt);
				if (s)
					return true;
			}
			return false;
		}

		static Dictionary<string, Func<NSAppleEventDescriptor, NSAppleEventDescriptor, bool>> callBacks = new Dictionary<string, Func<NSAppleEventDescriptor, NSAppleEventDescriptor, bool>> ();
		public static void RegisterCallBack (string provider, Func<NSAppleEventDescriptor, NSAppleEventDescriptor, bool> func)
		{
			callBacks [provider] = func;
		}

		class CallBackHelper : NSObject
		{
			public CallBackHelper ()
			{
				NSAppleEventManager.SharedAppleEventManager.SetEventHandler (this, new ObjCRuntime.Selector ("UrlHandleEvent:event:replyEvent"), AEEventClass.Internet, AEEventID.GetUrl);
			}

			[Export ("UrlHandleEvent:event:replyEvent")]
			public void UrlHandleEvent (NSAppleEventDescriptor evt, NSAppleEventDescriptor replyEvt)
			{
				Native.UrlHandleEvent (evt,replyEvt);
			}

		}
	}
}
