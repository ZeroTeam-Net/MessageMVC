using Agebull.EntityModel.Common;
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
        public IMessageItem Message { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        public TraceInfo Trace { get; set; }

        /// <summary>
        /// 全局状态
        /// </summary>
        public ContextStatus Status { get; set; }

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        public DependencyObjects DependencyObjects { get; } = new DependencyObjects();

    }
}
