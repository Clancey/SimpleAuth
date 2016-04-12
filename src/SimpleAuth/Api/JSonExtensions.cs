using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SimpleAuth
{
	public static class JSonExtensions
	{
		public static bool ContainsKey(this JObject jobj,string key)
		{
			return jobj [key] != null;
		}
		public static string ToJson(this object obj)
		{
			var json = obj as string;
			if (!string.IsNullOrEmpty(json))
				return json;
			return JsonConvert.SerializeObject (obj);
		}
		public static Task<string> ToJsonAsync(this object obj)
		{
			return Task.Run(()=>obj.ToJson());
		}

		public static T ToObject<T> (this string str)
		{
			return JsonConvert.DeserializeObject<T>(str);
		}

		public static Task<T> ToObjectAsync<T> (this string str)
		{
			return Task.Run(()=> JsonConvert.DeserializeObject<T>(str));
		}
	}
}

