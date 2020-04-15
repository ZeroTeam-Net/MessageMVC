﻿using Agebull.Common.Ioc;
using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 标准消息返回
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MessageResult : IMessageResult
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ID { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public MessageResult()
        {
            //防止反序列化构造失败
            RuntimeStatus = DependencyHelper.Create<IOperatorStatus>();
        }

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
        /// 处理状态
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public MessageState State { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public TraceInfo Trace { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ResultData { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IOperatorStatus RuntimeStatus { get; set; }
    }
}