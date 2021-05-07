using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class Cookies
    {
        private readonly Dictionary<string, string> mItems = new();

        public string this[string name]
        {
            get
            {
                return GetValue(name);
            }
        }

        public void Clear()
        {
            mItems.Clear();
        }

        public IDictionary<string, string> Copy()
        {
            Dictionary<string, string> result = new();
            foreach (var item in mItems)
                result[item.Key] = item.Value;
            return result;
        }

        private string GetValue(string name)
        {
            mItems.TryGetValue(name, out string result);
            return result;
        }

        internal void Add(string name, string value)
        {
            name = System.Web.HttpUtility.UrlDecode(name);
            value = System.Web.HttpUtility.UrlDecode(value);
            mItems[name] = value;
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var item in mItems)
            {
                sb.AppendFormat("{0}={1}\r\n", item.Key, item.Value);
            }
            return sb.ToString();
        }
    }
}
