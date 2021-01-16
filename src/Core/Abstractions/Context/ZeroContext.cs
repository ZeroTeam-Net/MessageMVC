using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;
using ActionTask = System.Threading.Tasks.TaskCompletionSource<(ZeroTeam.MessageMVC.Messages.MessageState state, object result)>;

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
            Status = new ContextStatus();
            Option = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     是否延迟处理
        /// </summary>
        public bool IsDelay { get; set; }

        /// <summary>
        /// 依赖范围
        /// </summary>
        public IDisposable DependencyScope { get; set; }

        /// <summary>
        /// 上下文配置
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Option { get; }

        /// <summary>
        /// 当前消息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public IInlineMessage Message { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public TraceInfo Trace => Message?.Trace;

        /// <summary>
        /// 全局状态
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public ContextStatus Status { get; }

        /// <summary>
        /// 当前任务，用于提前返回
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public ActionTask Task { get; set; }

        /// <summary>
        ///     当前用户
        /// </summary>
        public IUser User { get; set; }

        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        public Dictionary<string, string> ToTransfer()
        {
            if (!Option.ContainsKey("User"))
                Option.Add("User", User.ToJson());
            return Option;
        }
    }
}
