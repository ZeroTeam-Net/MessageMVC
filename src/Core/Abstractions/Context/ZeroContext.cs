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
            User = DependencyHelper.GetService<IUser>();//防止反序列化失败
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

        private IInlineMessage message;
        /// <summary>
        /// 当前消息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public IInlineMessage Message { get => message; set => message = value; }

        private TraceInfo trace;

        /// <summary>
        ///     跟踪信息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public TraceInfo Trace
        {
            get
            {
                return ZeroAppOption.Instance.TraceInfo == TraceInfoType.None || trace != null 
                    ? trace 
                    : trace = TraceInfo.New(Message?.ID);
            }
            set => trace = value;
        }

        /// <summary>
        /// 全局状态
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public ContextStatus Status { get; set; }

    }
}
