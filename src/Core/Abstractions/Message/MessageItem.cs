﻿using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息交互格式
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MessageItem : IMessageItem
    {
        /// <summary>
        /// 消息标识
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ID { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public MessageState State { get; set; }

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

        /*// <summary>
        /// 其他二进制内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, byte[]> Binary { get; set; }*/


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
        ///     跟踪信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public TraceInfo Trace { get; set; }

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
