using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Context
{

    /// <summary>
    ///     全局上下文
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public static class GlobalContext
    {
        #region 上下文对象处理

        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static IZeroContext Current => DependencyScope.Dependency.TryGetDependency(DependencyHelper.GetService<IZeroContext>);

        /// <summary>
        ///     当前线程的上下文中的对象
        /// </summary>
        public static T Get<T>() where T : class
        {
            return DependencyScope.Dependency.Dependency<T>();
        }

        /// <summary>
        ///     当前线程的上下文中的对象
        /// </summary>
        public static T Set<T>(T value) where T : class
        {
            return DependencyScope.Dependency.TryAnnex<T>(value);
        }

        /// <summary>
        ///     当前线程的调用上下文(无懒构造)
        /// </summary>
        public static IZeroContext CurrentNoLazy => DependencyScope.Dependency.Dependency<IZeroContext>();

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        public static IZeroContext Reset(IInlineMessage message)
        {
            var ctx = DependencyHelper.GetService<IZeroContext>();
            ctx.Message = message;
            if (message.Context != null)
            {
                foreach (var kv in message.Context)
                {
                    ctx.Option[kv.Key] = kv.Value;
                }
            }
            DependencyScope.Dependency.Annex(ctx);
            return ctx;
        }

        #endregion

        #region 上下文配置

        /// <summary>
        ///     启用调用链跟踪,默认为AppOption中的设置, 可通过远程传递而扩散
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public static bool EnableLinkTrace
        {
            get
            {
                var lazy = CurrentNoLazy;
                return ZeroAppOption.Instance.TraceInfo.HasFlag(TraceInfoType.LinkTrace) ||
                (lazy != null && lazy.Trace.ContentInfo.HasFlag(TraceInfoType.LinkTrace));
            }
        }

        /// <summary>
        /// 上下文配置指定名称是否配置为true
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <param name="destValue">用于对比的目标值</param>
        /// <param name="comparison">文本比较方式(默认为忽略大小写)</param>
        /// <returns></returns>
        public static bool IsOptionEquals(string name, string destValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (CurrentNoLazy?.Option == null ||
                CurrentNoLazy.Option.TryGetValue(name, out var op) != true
                || string.IsNullOrEmpty(op))
                return false;
            return string.Equals(op, destValue, comparison);
        }

        /// <summary>
        /// 上下文配置指定名称是否配置为true
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns></returns>
        public static bool IsOptionTrue(string name)
        {
            return IsOptionEquals(name, "true");
        }
        /// <summary>
        /// 取子网上下文配置指定名称的内容(不存在则为空)
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns></returns>
        public static string GetOption(string name)
        {
            if (CurrentNoLazy?.Option == null ||
                CurrentNoLazy.Option.TryGetValue(name, out var op) != true)
                return null;
            return op;
        }

        #endregion
    }
}
