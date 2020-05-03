using Agebull.Common.Ioc;
using System;
using System.Text.Json;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 智能序列化器
    /// </summary>
    public static class SmartSerializer
    {
        /// <summary>
        /// 内置序列化器
        /// </summary>
        public static readonly JsonSerializeProxy MsJson = new JsonSerializeProxy();


        static readonly IJsonSerializeProxy Json = DependencyHelper.Create<IJsonSerializeProxy>();
        static readonly IXmlSerializeProxy Xml = DependencyHelper.Create<IXmlSerializeProxy>();
        static readonly Type MessageType = DependencyHelper.Create<IInlineMessage>().GetType();

        /// <summary>
        /// 自动序列化()
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>字符</returns>
        /// <remarks>
        /// 基于内部全部使用JSON传输的规则,如序列化器不存在,则为JSON
        /// </remarks>
        public static string SerializeMessage(IMessageItem message)
        {
            if (message == null)
                return null;
            return JsonSerializer.Serialize(new MessageItem
            {
                ID = message.ID,
                State = message.State,
                Topic = message.Topic,
                Title = message.Title,
                Content = message.Content,
                Result = message.Result,
                Trace = message.Trace
            });
        }

        /// <summary>
        /// 自动序列化()
        /// </summary>
        /// <param name="result">消息</param>
        /// <returns>字符</returns>
        /// <remarks>
        /// 基于内部全部使用JSON传输的规则,如序列化器不存在,则为JSON
        /// </remarks>
        public static string SerializeResult(IMessageResult result)
        {
            if (result == null)
                return null;
            return JsonSerializer.Serialize(new MessageResult
            {
                ID = result.ID,
                State = result.State,
                Trace = result.Trace,
                Result = result.Result,
                DataState = result.Result != null ? MessageDataState.ResultOffline : MessageDataState.None
            });
        }

        /// <summary>
        /// 自动序列化为MessageResult
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>字符</returns>
        /// <remarks>
        /// 基于内部全部使用JSON传输的规则,如序列化器不存在,则为JSON
        /// </remarks>
        public static string SerializeResult(IInlineMessage message)
        {
            return JsonSerializer.Serialize(message.ToMessageResult(true));
        }

        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>对象</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static IInlineMessage ToMessage(string str, bool trim = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            if (trim)
                str = str.Trim();
            IInlineMessage message;
            switch (str[0])
            {
                case '{':
                case '[':
                    message = MsJson.ToObject(str, MessageType) as IInlineMessage;
                    break;
                case '<':
                    message = Xml.ToObject(str, MessageType) as IInlineMessage;
                    break;
                default:
                    return null;
            }
            message.DataState = MessageDataState.ArgumentOffline;
            return message;
        }

        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="message">返回的消息</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>是否成功</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static bool TryToMessage(string str, out IInlineMessage message, bool trim = true)
        {
            try
            {
                message = ToMessage(str, trim);
                return message != null;
            }
            catch
            {
                message = null;
                return false;
            }
        }
        /// <summary>
        /// 使用MsJson序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>字符</returns>
        /// <remarks>
        /// 基于内部全部使用JSON传输的规则,如序列化器不存在,则为JSON
        /// </remarks>
        public static string ToInnerString(object obj)
        {
            return obj == null
                ? null
                : MsJson.ToString(obj, false);
        }
        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>对象</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static T FromInnerString<T>(string str, bool trim = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }
            if (trim)
                str = str.Trim();
            switch (str[0])
            {
                case '{':
                case '[':
                    return MsJson.ToObject<T>(str);
                case '<':
                    return Xml.ToObject<T>(str);
                default:
                    return default;
            }
        }

        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="obj">返回的对象</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>对象</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static bool TryFromInnerString<T>(string str, out T obj, bool trim = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                obj = default;
                return false;
            }
            if (trim)
                str = str.Trim();
            switch (str[0])
            {
                case '{':
                case '[':
                    obj = MsJson.ToObject<T>(str);
                    break;
                case '<':
                    obj = Xml.ToObject<T>(str);
                    break;
                default:
                    obj = default;
                    break;
            }
            return Equals(obj, default);
        }

        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="type">类型</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>对象</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static object FromInnerString(string str, Type type, bool trim = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            if (trim)
                str = str.Trim();
            switch (str[0])
            {
                case '{':
                case '[':
                    return MsJson.ToObject(str, type);
                case '<':
                    return Xml.ToObject(str, type);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 自动序列化()
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="serializer">序列化器</param>
        /// <returns>字符</returns>
        /// <remarks>
        /// 基于内部全部使用JSON传输的规则,如序列化器不存在,则为JSON
        /// </remarks>
        public static string ToString(object obj, ISerializeProxy serializer = null)
        {
            return obj == null
                ? null
                : serializer != null
                    ? serializer.ToString(obj)
                    : Json.ToString(obj);
        }

        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="type">类型</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>对象</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static object ToObject(string str, Type type, bool trim = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            if (trim)
                str = str.Trim();
            switch (str[0])
            {
                case '{':
                case '[':
                    return Json.ToObject(str, type);
                case '<':
                    return Xml.ToObject(str, type);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 自动根据字符特点反序列化
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="trim">执行Trim,以消除空白字符</param>
        /// <returns>对象</returns>
        /// <remarks>
        /// xml以&lt;为第一个字符
        /// json以[或{为第一个字符
        /// </remarks>
        public static T ToObject<T>(string str, bool trim = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }
            if (trim)
                str = str.Trim();
            switch (str[0])
            {
                case '{':
                case '[':
                    return Json.ToObject<T>(str);
                case '<':
                    return Xml.ToObject<T>(str);
                default:
                    return default;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static bool TryDeserialize(string soruce, Type type, out object dest)
        {
            if (soruce == null)
            {
                dest = default;
                return false;
            }
            try
            {
                dest = ToObject(soruce, type);
                return Equals(dest, default);
            }
            catch
            {
                dest = default;
                return false;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static bool TryDeserialize<T>(string soruce, out T dest)
        {
            if (soruce == null)
            {
                dest = default;
                return false;
            }
            try
            {
                dest = ToObject<T>(soruce);
                return !Equals(dest, default);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                dest = default;
                return false;
            }
        }
    }

}