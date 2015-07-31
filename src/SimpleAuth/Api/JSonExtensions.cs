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
			return JsonConvert.SerializeObject (obj);
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

