using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace SimpleAuth {
	public static class JSonExtensions {
		public static bool ContainsKey (this JObject jobj, string key)
		{
			return jobj [key] != null;
		}
		public static string ToJson (this object obj)
		{
			var json = obj as string;
			if (!string.IsNullOrEmpty (json))
				return json;
			return JsonConvert.SerializeObject (obj);
		}
		public static Task<string> ToJsonAsync (this object obj)
		{
			return Task.Run (() => obj.ToJson ());
		}

		public static T ToObject<T> (this string str)
		{
			if (typeof (T) == str.GetType ())
				return (T)(object)str;
			if (string.IsNullOrWhiteSpace (str))
				return default (T);
			return JsonConvert.DeserializeObject<T> (str);
		}
		public static T ToObject<T> (this string str, object inObject)
		{
			if (inObject is T && inObject != null) {
				var serializer = new Newtonsoft.Json.JsonSerializer ();
				using (var reader = new StringReader (str)) {
					var outObj = (T)inObject;
					serializer.Populate (reader, outObj);
					return outObj;
				}
			}
			return str.ToObject<T> ();
		}
		public static Task<T> ToObjectAsync<T> (this string str)
		{
			return Task.Run (() => str.ToObject<T> ());
		}
	}
}

