using Newtonsoft.Json;
using System;
using System.Xml;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// Xml序列化装饰器
    /// </summary>
    public class XmlSerializeProxy : IXmlSerializeProxy
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public object Serialize(object obj)
        {
            return ToString(obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj">源内容(一般都是文本)</param>
        /// <param name="type">类型</param>
        /// <returns>结果对象，可能因为格式不良好而产生异常</returns>
        public object Deserialize(object obj, Type type)
        {
            return ToString(obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public T ToObject<T>(string xml)
        {
            return (T)ToObject(xml, typeof(T));
        }
        ///<inheritdoc/>
        public object ToObject(string xml, Type type)
        {
            xml = xml.XmlTrim();
            if (xml == null)
            {
                return default;
            }
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, true);
            if (json == "null")
                json = "{}";
            return JsonConvert.DeserializeObject(json, type);
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented = false)
        {
            if (obj == null)
                return null;
            var json = JsonConvert.SerializeObject(obj);
            var doc = JsonConvert.DeserializeXmlNode(json, "xml", true, true);
            return doc.OuterXml;
        }
    }
}