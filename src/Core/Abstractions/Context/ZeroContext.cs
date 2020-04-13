using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///     全局上下文
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ZeroContext : IZeroContext
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ZeroContext()
        {
            User = DependencyHelper.Create<IUser>();//防止反序列化失败
            Status = new ContextStatus();
            Option = new Dictionary<string, string>();
        }

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
        public IInlineMessage Message { get; set; }

        private TraceInfo trace;

        /// <summary>
        ///     跟踪信息
        /// </summary>
        public TraceInfo Trace { get => trace ??= new TraceInfo(); set => trace = value; }

        /// <summary>
        /// 全局状态
        /// </summary>
        public ContextStatus Status { get; set; }

    }
}
