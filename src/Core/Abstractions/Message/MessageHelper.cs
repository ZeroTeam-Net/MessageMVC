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
        public static IInlineMessage Simple(string id, string topic, string title, string content)
        {
            return new InlineMessage
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
        public static IInlineMessage Restore(string topic, string title, string content, string id, string context)
        {
            return new InlineMessage
            {
                Topic = topic,
                Title = title,
                Content = content,
                Trace = new TraceInfo
                {
                    TraceId = id,
                    Context = JsonHelper.DeserializeObject<IZeroContext>(context)
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
        public static IInlineMessage NewMessage<T>(string topic, string title, T content)
        {
            return new InlineMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Topic = topic,
                Title = title,
                ArgumentData = content
            };
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
        public static InlineMessage NewRemote<T>(string topic, string title, T content)
        {
            var msg = new InlineMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Topic = topic,
                Title = title,
                ArgumentData = content
            };
            if (GlobalContext.EnableLinkTrace)
            {
                msg.Trace = new TraceInfo
                {
                    TraceId = msg.ID,
                    Start = DateTime.Now,
                };
                msg.Trace.CopyFromContext();
            }
            return msg;
        }

        /// <summary>
        /// 构造一个远程调用的消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static InlineMessage NewRemote(string topic, string title, string content = null)
        {
            var msg = new InlineMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Topic = topic,
                Title = title,
                Content = content
            };
            if (GlobalContext.EnableLinkTrace)
            {
                msg.Trace = new TraceInfo
                {
                    TraceId = msg.ID,
                    Start = DateTime.Now,
                };
                msg.Trace.CopyFromContext();
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
