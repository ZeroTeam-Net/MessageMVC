using Agebull.Common;
using System;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息辅助类
    /// </summary>
    public static class MessageHelper
    {

        /// <summary>
        /// 简单消息
        /// </summary>
        /// <param name="id">跟踪标识</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IMessageItem Simple(string id, string topic, string title, string content)
        {
            return new MessageItem
            {
                ID = id,
                Topic = topic,
                Title = title,
                Content = content
            };
        }

        /// <summary>
        /// 恢复
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <param name="id">跟踪标识</param>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public static IMessageItem Restore(string topic, string title, string content, string id, string context)
        {
            return new MessageItem
            {
                Topic = topic,
                Title = title,
                Content = content,
                Trace = new TraceInfo
                {
                    TraceId = id,
                    ContextJson = context
                }
            };
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IMessageItem NewMessage<T>(string topic, string title, T content)
        {
            return NewMessage(topic, title, JsonHelper.SerializeObject(content));
        }


        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IMessageItem NewMessage(string topic, string title, string content = null)
        {
            return new MessageItem
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Topic = topic,
                Title = title,
                Content = content
            };
        }

        /// <summary>
        /// 构造一个远程调用的消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static MessageItem NewRemote<T>(string topic, string title, T content)
        {
            return NewRemote(topic, title, JsonHelper.SerializeObject(content));
        }

        /// <summary>
        /// 构造一个远程调用的消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static MessageItem NewRemote(string topic, string title, string content = null)
        {
            var id = Guid.NewGuid().ToString("N").ToUpper();
            var msg = new MessageItem
            {
                ID = id,
                Topic = topic,
                Title = title,
                Content = content
            };
            if (GlobalContext.EnableLinkTrace)
            {
                //远程机器使用,所以Call是本机信息
                msg.Trace = new TraceInfo()
                {
                    CallTimestamp = DateTime.Now.ToTimestamp(),
                    CallApp = $"{ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})",
                    CallMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})"
                };
                if(GlobalContext.CurrentNoLazy != null)
                {
                    msg.Trace.TraceId = GlobalContext.Current.Trace.TraceId;
                    msg.Trace.CallId = GlobalContext.Current.Trace.LocalId;
                    msg.Trace.Token = GlobalContext.Current.Trace.Token;
                    msg.Trace.Headers = GlobalContext.Current.Trace.Headers;
                    msg.Trace.Ip = GlobalContext.Current.Trace.Ip;
                    msg.Trace.Ip = GlobalContext.Current.Trace.Ip;
                    msg.Trace.Port = GlobalContext.Current.Trace.Port;
                    msg.Trace.ContextJson = JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy);
                }
                else
                {
                    msg.Trace.TraceId = id;
                }
            }
            return msg;
        }
    }
}

/*// <summary>
///     文件内容二进制数据
/// </summary>
public byte[] Bytes { get; set; }

/// <summary>
/// 生产者信息
/// </summary>
[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
public string ProducerInfo { get; set; }

/// <summary>
/// 构造
/// </summary>
/// <returns></returns>
public MessageItem()
{

}*/
