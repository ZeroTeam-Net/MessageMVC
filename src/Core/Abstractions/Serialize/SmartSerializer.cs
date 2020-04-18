using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 智能序列化器
    /// </summary>
    public static class SmartSerializer
    {
        static readonly IJsonSerializeProxy Json = new JsonSerializeProxy();
        static readonly IXmlSerializeProxy Xml = DependencyHelper.Create<IXmlSerializeProxy>();
        static readonly Type MessageType = DependencyHelper.Create<IInlineMessage>().GetType();

        /// <summary>
        /// 自动序列化()
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="serializer">序列化器</param>
        /// <returns>字符</returns>
        /// <remarks>
        /// 基于内部全部使用JSON传输的规则,如序列化器不存在,则为JSON
        /// </remarks>
        public static string SerializeMessage(IMessageItem message, ISerializeProxy serializer = null)
        {
            if (message == null)
                return null;
            var msg = new MessageItem
            {
                ID = message.ID,
                State = message.State,
                Topic = message.Topic,
                Title = message.Title,
                Content = message.Content,
                Result = message.Result
            };
            if(message.Trace != null)
            {
                msg.Trace = new TraceInfo
                {
                    TraceId = message.Trace.TraceId,
                    Start = message.Trace.Start,
                    LocalId = message.Trace.LocalId,
                    LocalApp = message.Trace.LocalApp,
                    LocalMachine = message.Trace.LocalMachine,
                    CallId = message.Trace.CallId,
                    CallApp = message.Trace.CallApp,
                    CallMachine = message.Trace.CallMachine,
                    Headers = message.Trace.Headers,
                    Token = message.Trace.Token,
                    Level = message.Trace.Level
                };
                if(message.Trace.Context != null)
                {
                    msg.Trace.Context = new StaticContext
                    {
                        User = message.Trace.Context.User,
                        Option = message.Trace.Context.Option
                    };
                }
                else
                {
                    msg.Trace.Context = null;
                }
            }
            return serializer != null
                    ? serializer.ToString(msg)
                    : Json.ToString(msg);
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
                    message = Json.ToObject(str, MessageType) as IInlineMessage;
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
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
                dest = default;
                return false;
            }
        }
    }

    /// <summary>
    ///     全局上下文(用于序列化)
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class StaticContext : IZeroContext
    {
        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IUser User { get; set; }

        /// <summary>
        /// 上下文配置
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Option { get; set; }

        /// <summary>
        /// 当前消息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public IInlineMessage Message { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public TraceInfo Trace { get; set; }

        /// <summary>
        /// 全局状态
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public ContextStatus Status { get; set; }

    }
}