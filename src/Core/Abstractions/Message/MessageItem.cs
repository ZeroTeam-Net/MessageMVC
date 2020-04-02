using Agebull.Common;
using Newtonsoft.Json;
using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息交互格式
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MessageItem : IMessageItem
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MessageItem()
        {

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
                Topic = topic,
                Title = title,
                Content = content
            };
        }

        /// <summary>
        /// 唯一标识，UUID
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 处理状态
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public MessageState State { get; set; }

        /// <summary>
        /// 生产时间戳,UNIX时间戳,自1970起秒数
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Timestamp { get; set; } = DateTime.Now.ToTimestamp();

        /// <summary>
        /// 分类
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Topic { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Content { get; set; }

        /// <summary>
        /// 处理结果,对应状态的解释信息
        /// </summary>
        /// <remarks>
        /// 未消费:无内容
        /// 已接受:无内容
        /// 格式错误 : 无内容
        /// 无处理方法 : 无内容
        /// 处理异常 : 异常信息
        /// 处理失败 : 失败内容或原因
        /// 处理成功 : 结果信息或无
        /// </remarks>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Result { get; set; }

        /// <summary>
        /// 其他带外内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Extend { get; set; }

        /// <summary>
        /// 上下文信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Context { get; set; }

        /// <summary>
        /// 服务名称,即Topic
        /// </summary>
        public string ServiceName => Topic;

        /// <summary>
        /// 接口名称,即Title
        /// </summary>
        public string ApiName => Title;

        /// <summary>
        /// 接口参数,即Content
        /// </summary>
        public string Argument => Content;


        /*// <summary>
        ///     文件内容二进制数据
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 生产者信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ProducerInfo { get; set; }*/

    }

}
