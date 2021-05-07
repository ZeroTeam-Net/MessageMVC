using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    class DataContextBind
    {
        public static void BindJson(IDataContext context, JToken data)
        {
            if (data != null)
            {
                context.SetValue("body", data);
                if (data is JObject)
                {
                    foreach (JProperty property in data)
                    {
                        if (property.Value != null)
                        {
                            context.SetValue(property.Name, property);
                        }
                    }
                }
            }
        }

        public static void BindFormUrl(IDataContext context, ReadOnlySpan<char> data)
        {
            HttpParse.AsynczeFromUrlEncoded(data, context);
        }

        public static DataConvertAttribute GetConvertAttribute(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return new JsonDataConvertAttribute();
            }
            else if (contentType.Contains("application/x-www-form-urlencoded", StringComparison.CurrentCultureIgnoreCase))
            {
                return new FormUrlDataConvertAttribute();
            }
            else if (contentType.Contains("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
            {
                return new MultiDataConvertAttribute();
            }
            else
            {
                return new JsonDataConvertAttribute();
            }
        }
    }
}
