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