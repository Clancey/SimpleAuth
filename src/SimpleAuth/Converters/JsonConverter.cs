//
//  Copyright 2020  
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
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleAuth {
	public class JsonConverter : Converter {
		
		static readonly JsonSerializer serializer = new JsonSerializer ();
		public override string MediaType { get; set; } = "application/json";
		public override async Task<T> Deserialize<T> (HttpResponseMessage response)
		{
			if (typeof (T) == typeof (string))
				return (T)(object)(await response.Content.ReadAsStringAsync ());
			var stream = await response.Content.ReadAsStreamAsync ();
			return await Task.Run (() => serializer.Deserialize<T> (new JsonTextReader (new StreamReader (stream))));
		}

		public override async Task<HttpContent> Serialize (object data)
		{
			var bodyJson = await data.ToJsonAsync ();
			var content = new StringContent (bodyJson, System.Text.Encoding.UTF8, MediaType);
			return content;
		}
	}
}
