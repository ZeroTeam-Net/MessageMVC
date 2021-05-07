using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class QueryString
    {

        public QueryString(Data.IDataContext dataContext)
        {
            mDataContext = dataContext;
        }

        private readonly Data.IDataContext mDataContext;


        public string this[string name]
        {
            get
            {
                return GetValue(name);
            }
        }

        private string GetValue(string name)
        {
            mDataContext.TryGetString(name, out string result);
            return result;


        }

        internal void Add(string name, string value)
        {
            mDataContext.SetValue(name, System.Web.HttpUtility.UrlDecode(value));
           
        }
    }
}
