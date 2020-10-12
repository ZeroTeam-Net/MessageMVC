using Newtonsoft.Json;
using System;
using System.Xml;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// Xml序列化装饰器
    /// </summary>
    public class CDataXmlSerializeProxy : IXmlSerializeProxy
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
            var xdoc = new XmlDocument();
            void ReplaceCData(XmlNodeList elements, XmlElement xElement)
            {
                foreach (var node in elements)
                {
                    if (node is XmlText text)
                    {
                        xElement.AppendChild(xdoc.CreateTextNode(text.Value));
                    }
                    else if (node is XmlCDataSection cdate)
                    {
                        xElement.AppendChild(xdoc.CreateTextNode(cdate.Value));
                    }
                    else if (node is XmlElement element)
                    {
                        var nElement = xdoc.CreateElement(element.LocalName);
                        ReplaceCData(element.ChildNodes, nElement);
                        xElement.AppendChild(nElement);
                    }
                }
            }
            var xroot = xdoc.CreateElement("xml");
            if (doc.ChildNodes.Count > 0)
            {
                ReplaceCData(doc.ChildNodes[0].ChildNodes, xroot);
            }
            xdoc.AppendChild(xroot);

            var json = JsonConvert.SerializeXmlNode(xdoc, Newtonsoft.Json.Formatting.None, true);

            return JsonConvert.DeserializeObject(json, type);
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented = false)
        {
            if (obj == null)
                return null;
            var json = JsonConvert.SerializeObject(obj);
            var doc = JsonConvert.DeserializeXmlNode(json, "xml", true, true);


            var xdoc = new XmlDocument();
            var xroot = xdoc.CreateElement("xml");
            xdoc.AppendChild(xroot);
            void ReplaceCData(XmlNodeList elements, XmlElement xElement)
            {
                foreach (var node in elements)
                {
                    if (node is XmlText text)
                    {
                        xElement.AppendChild(xdoc.CreateCDataSection(text.Value));
                    }
                    else if (node is XmlCDataSection cdata)
                    {
                        xElement.AppendChild(xdoc.CreateCDataSection(cdata.Value));
                    }
                    else if (node is XmlElement element)
                    {
                        var nElement = xdoc.CreateElement(element.LocalName);
                        xElement.AppendChild(nElement);
                        ReplaceCData(element.ChildNodes, nElement);
                    }
                }
            }
            if (doc.ChildNodes.Count > 0)
                ReplaceCData(doc.ChildNodes[0].ChildNodes, xroot);
            return xdoc.OuterXml;
        }
    }
    /*// <summary>
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
            return ToString(obj, obj?.GetType());
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj">源内容(一般都是文本)</param>
        /// <param name="type">类型</param>
        /// <returns>结果对象，可能因为格式不良好而产生异常</returns>
        public object Deserialize(object obj, Type type)
        {
            return ToString(obj, type);
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
            if (string.IsNullOrWhiteSpace(xml))
            {
                return default;
            }
            byte[] buffers;
            long len;
            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms);
                sw.Write(xml);
                sw.Flush();
                len = ms.Position;
                buffers = ms.GetBuffer();
            }
            using var reader = XmlDictionaryReader.CreateTextReader(buffers,
                0,
                (int)len,
                new XmlDictionaryReaderQuotas());
            var ds = new DataContractSerializer(type);
            return ds.ReadObject(reader, false);
        }

        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return ToString(obj, obj?.GetType());
        }

        ///<inheritdoc/>
        public string ToString(object obj, Type type)
        {
            if (obj == null)
                return null;
            using (var ms = new MemoryStream())
            {
                var ds = new DataContractSerializer(type);
                ds.WriteObject(ms, obj);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }
    }*/
}