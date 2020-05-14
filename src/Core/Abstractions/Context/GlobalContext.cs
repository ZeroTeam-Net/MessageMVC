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
        ///     当前线程的调用上下文(无懒构造)
        /// </summary>
        public static IZeroContext CurrentNoLazy => DependencyScope.Dependency.Dependency<IZeroContext>();

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        public static void SetContext(IInlineMessage message)
        {
            var ctx = DependencyHelper.GetService<IZeroContext>();
            ctx.Message = message;
            ctx.Trace = message.Trace;
            ctx.User.FormJson(message.Trace?.Context?.UserJson);
            DependencyScope.Dependency.Annex(ctx);
        }

        /// <summary>
        ///     内部构造
        /// </summary>
        public static IZeroContext Reset() => DependencyScope.Dependency.Annex(DependencyHelper.GetService<IZeroContext>());

        /// <summary>
        ///     置空并注销当前上下文
        /// </summary>
        public static void SetEmpty() => DependencyScope.Dependency.Remove<IZeroContext>();

        #endregion

        #region 用户
        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static IUser User => Current.User;

        /// <summary>
        /// 表示一个匿名用户
        /// </summary>
        public static IUser Anymouse { get; } = DependencyHelper.GetService<IUser>();

        #endregion

        #region 上下文配置

        static bool enableLinkTrace;

        /// <summary>
        ///     启用调用链跟踪,默认为AppOption中的设置, 可通过远程传递而扩散
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public static bool EnableLinkTrace
        {
            set => enableLinkTrace = value;
            get => enableLinkTrace || IsOptionTrue("EnableLinkTrace");
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
