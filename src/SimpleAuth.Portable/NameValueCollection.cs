using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace System.Collections.Specialized
{
    public class NameValueCollection : Collection<HttpValue>
    {
        #region Parameters

        public string this[string key]
        {
            get { return this.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value; }
            set {
                var existingKey = this.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
                if (existingKey != null)
                    existingKey.Value = value;
                else
                    this.Add(new HttpValue(key, value));
            }
        }

        #endregion

        #region Private Methods

        void FillFromString(string query, bool urlencoded)
        {
            var num = query?.Length ?? 0;
            for (var i = 0; i < num; i++)
            {
                var startIndex = i;
                var num4 = -1;
                while (i < num)
                {
                    var ch = query[i];
                    if (ch == '=')
                    {
                        if (num4 < 0)
                        {
                            num4 = i;
                        }
                    }
                    else if (ch == '&')
                    {
                        break;
                    }
                    i++;
                }
                string str = null;
                string str2 = null;
                if (num4 >= 0)
                {
                    str = query.Substring(startIndex, num4 - startIndex);
                    str2 = query.Substring(num4 + 1, (i - num4) - 1);
                }
                else
                {
                    str2 = query.Substring(startIndex, i - startIndex);
                }

                if (urlencoded)
                {
                    Add(Uri.UnescapeDataString(str), Uri.UnescapeDataString(str2));
                }
                else
                {
                    Add(str, str2);
                }

                if ((i == (num - 1)) && (query[i] == '&'))
                {
                    Add(null, string.Empty);
                }
            }
        }

        #endregion

        #region Constructors

        public NameValueCollection()
        {
        }

        public NameValueCollection(string query)
            : this(query, true)
        {
        }

        public NameValueCollection(string query, bool urlencoded)
        {
            if (!string.IsNullOrEmpty(query))
            {
                FillFromString(query, urlencoded);
            }
        }

        #endregion

        #region Public Methods

        public void Add(string key, string value)
        {
            Add(new HttpValue(key, value));
        }

        public bool ContainsKey(string key)
        {
            return this.Any(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        public string[] GetValues(string key)
        {
            return this.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToArray();
        }

        public void Remove(string key)
        {
            foreach (var x in this.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                Remove(x);
            }
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public virtual string ToString(bool urlencoded)
        {
            return ToString(urlencoded, null);
        }

        public virtual string ToString(bool urlencoded, IDictionary excludeKeys)
        {
            if (Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            foreach (var item in this)
            {
                var key = item.Key;

                if ((excludeKeys != null) && excludeKeys.Contains(key)) continue;
                var value = item.Value;

                if (urlencoded)
                {
                    key = WebUtility.UrlDecode(key);
                }

                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append('&');
                }

                stringBuilder.Append((key != null) ? (key + "=") : string.Empty);

                if (string.IsNullOrEmpty(value)) continue;
                if (urlencoded)
                {
                    value = Uri.EscapeDataString(value);
                }

                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }

        #endregion
    }
}
