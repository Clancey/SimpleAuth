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
using Foundation;

namespace Foundation
{
	public static class AppleEventParameters
	{
		static uint FourCC (string s)
		{
			return (uint)(((int)s [0]) << 24 |
					((int)s [1]) << 16 |
					((int)s [2]) << 8 |
					((int)s [3]));
		}


		public static uint DirectObject => FourCC (keyDirectObject);
		public static uint ErrorNumber => FourCC (keyErrorNumber);
		public static uint ErrorString => FourCC (keyErrorString);
		public static uint ProcessSerialNumber => FourCC (keyProcessSerialNumber);
		public static uint PreDispatch => FourCC (keyPreDispatch);
		public static uint SelectProc => FourCC (keySelectProc);
		public static uint AERecorderCount => FourCC (keyAERecorderCount);
		public static uint AEVersion => FourCC (keyAEVersion);


		/* Keywords for Apple event parameters */
		const string keyDirectObject = "----";
		const string keyErrorNumber = "errn";
		const string keyErrorString = "errs";
		const string keyProcessSerialNumber = "psn "; /* Keywords for special handlers */
		const string keyPreDispatch = "phac"; /* preHandler accessor call */
		const string keySelectProc = "selh"; /* more selector call */
											 /* Keyword for recording */
		const string keyAERecorderCount = "recr"; /* available only in vers 1.0.1 and greater */
												  /* Keyword for version information */
		const string keyAEVersion = "vers"; /* available only in vers 1.0.1 and greater */
	}
}
