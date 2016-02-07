using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SimpleAuth
{
   public static class UrlUtility
    {
	   public static Uri AddParameters(this Uri url, string name,string value)
	   {
		   return url.AddParameters(new NameValueCollection
		   {
			   {name,value}
		   });
	   }
	   public static Uri AddParameters(this Uri url, NameValueCollection parameters)
	   {

			var query = url.Query;
			var simplePath = string.IsNullOrWhiteSpace(query) ? url.AbsoluteUri : url.AbsoluteUri.Replace(query, "");

		   var existingParams = HttpUtility.ParseQueryString(query);
		   foreach (string key in parameters)
		   {
			   existingParams[key] = parameters[key];
		   }
		   var newQuery = existingParams.ToString();
			var newPath = $"{simplePath}?{newQuery}";
			return new Uri(newPath);
		}
    }
}
