﻿using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Agebull.Common
{
    /// <summary>
    /// 对URL的扩展
    /// </summary>
    public class UrlHelper
    {
        /// <summary>
        /// 将查询字符串解析转换为名值集合.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static NameValueCollection GetQueryString(string queryString)
        {
            return GetQueryString(queryString, null, true);
        }

        /// <summary>
        /// 将查询字符串解析转换为名值集合.
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="encoding"></param>
        /// <param name="isEncoded"></param>
        /// <returns></returns>
        public static NameValueCollection GetQueryString(string queryString, Encoding encoding, bool isEncoded)
        {
            queryString = queryString.TrimStart('?', ' ', '\t');
            var result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(queryString))
                return result;
            var count = queryString.Length;
            for (var i = 0; i < count; i++)
            {
                var startIndex = i;
                var index = -1;
                while (i < count)
                {
                    var item = queryString[i];
                    if (item == '=')
                    {
                        if (index < 0)
                        {
                            index = i;
                        }
                    }
                    else if (item == '&')
                    {
                        break;
                    }
                    i++;
                }
                string key = null;
                string value = null;
                if (index >= 0)
                {
                    key = queryString[startIndex..index];
                    value = queryString.Substring(index + 1, i - index - 1);
                }
                else
                {
                    key = queryString[startIndex..i];
                }
                if (isEncoded)
                {
                    result[MyUrlDeCode(key, encoding)] = MyUrlDeCode(value, encoding);
                }
                else
                {
                    result[key] = value;
                }
                if ((i == (count - 1)) && (queryString[i] == '&'))
                {
                    result[key] = string.Empty;
                }
            }
            return result;
        }

        /// <summary>
        /// 解码URL.
        /// </summary>
        /// <param name="encoding">null为自动选择编码</param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MyUrlDeCode(string str, Encoding encoding)
        {
            if (encoding != null)
                return HttpUtility.UrlDecode(str, encoding);
            //首先用utf-8进行解码                     
            var code = HttpUtility.UrlDecode(str.ToUpper(), Encoding.UTF8);
            //将已经解码的字符再次进行编码.
            var encode = HttpUtility.UrlEncode(code, Encoding.UTF8)?.ToUpper();
            encoding = str == encode ? Encoding.UTF8 : Encoding.GetEncoding("gb2312");
            return HttpUtility.UrlDecode(str, encoding);
        }
    }
}
