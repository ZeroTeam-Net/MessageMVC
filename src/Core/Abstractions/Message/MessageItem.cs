﻿using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息交互格式
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MessageItem : IMessageItem
    {
        /// <summary>
        /// 唯一标识，UUID
        /// </summary>
        [JsonProperty()]
        public string ID { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        public MessageState State { get; set; }

        /// <summary>
        /// 生产时间戳,UNIX时间戳,自1970起秒数
        /// </summary>
        public int Timestamp { get; set; }

        /// <summary>
        /// 生产者信息
        /// </summary>
        public string ProducerInfo { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
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
        public string Result { get; set; }

        /// <summary>
        ///     文件内容二进制数据
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 其他带外内容
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 上下文信息
        /// </summary>
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
    }

}
