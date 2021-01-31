using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        #region IScopeContext

        ScopeAttachData scopeData;

        /// <summary>
        /// 范围数据
        /// </summary>
        ScopeAttachData IScopeContext.ScopeData
        {
            get => scopeData;
            set => scopeData = value;
        }

        /// <summary>
        /// 析构范围
        /// </summary>
        public ScopeAttachData ScopeData => scopeData;

        IDisposable scope;

        /// <summary>
        /// 析构范围
        /// </summary>
        IDisposable IScopeContext.Scope
        {
            get => scope;
            set => scope = value;
        }

        /// <summary>
        /// 析构范围
        /// </summary>
        public IDisposable Scope => scope;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IScopeContext IScopeContext.Clone()
        {
            var ctx = new ZeroContext
            {
                Message = Message,
                User = User
            };
            foreach (var kv in Option)
                ctx.Option.Add(kv.Key, kv.Value);
            return ctx;
        }

        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        Dictionary<string, string> IDictionaryTransfer.ToDictionary()
        {
            return Option.Count == 0 ? null : Option;
        }
        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        void IDictionaryTransfer.Reset(Dictionary<string, string> dict)
        {
            Option = dict ?? new Dictionary<string, string>();
        }

        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public ZeroContext()
        {
            Status = new ContextStatus();
            Option = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 上下文配置
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Option { get; private set; }

        /// <summary>
        /// 当前消息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public IInlineMessage Message { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public TraceInfo TraceInfo => Message?.TraceInfo;

        /// <summary>
        /// 全局状态
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public ContextStatus Status { get; }

        /// <summary>
        /// 当前任务，用于提前返回
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public ActionTask RequestTask { get; set; }

        /// <summary>
        /// 当前执行的任务，用于正确等待
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public TaskCompletionSource<bool> ActionTask { get; set; }

        /// <summary>
        /// 提前设置返回值，这会导致方法不中断，而框架则提前返回到消息调用处
        /// </summary>
        void IZeroContext.SetResult(MessageState state, object result)
        {
            RequestTask.TrySetResult((state, result));
        }

        /// <summary>
        ///     当前用户
        /// </summary>
        public IUser User
        {
            get => ScopeRuner.ScopeUser;
            set => ScopeRuner.ScopeUser = value;
        }

    }
}
