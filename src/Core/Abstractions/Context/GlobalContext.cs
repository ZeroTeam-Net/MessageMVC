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
        public static IZeroContext Current => (IZeroContext)(ScopeRuner.ScopeContext ??= DependencyHelper.GetService<IZeroContext>());

        /// <summary>
        ///     当前用户
        /// </summary>
        public static IUser User
        {
            get => ScopeRuner.ScopeUser;
            set => ScopeRuner.ScopeUser = value;
        }

        /// <summary>
        ///     当前消息
        /// </summary>
        public static IInlineMessage Message => ScopeRuner.ScopeContext == null ? null : ((IZeroContext)(ScopeRuner.ScopeContext)).Message;

        /// <summary>
        ///     当前线程的上下文中的对象
        /// </summary>
        public static T Get<T>() where T : class
        {
            return ScopeRuner.ScopeDependency.Get<T>();
        }

        /// <summary>
        ///     当前线程的上下文中的对象
        /// </summary>
        public static T Set<T>(T value) where T : class
        {
            return ScopeRuner.ScopeDependency.TryAttach<T>(value);
        }

        /// <summary>
        ///     当前线程的调用上下文(无懒构造)
        /// </summary>
        public static IZeroContext CurrentNoLazy => (IZeroContext)(ScopeRuner.ScopeContext);

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
            ScopeRuner.ScopeContext = ctx;
            ScopeRuner.ScopeUser = DependencyHelper.GetService<IUser>();
            ScopeRuner.ScopeUser?.Reset(message.User);
            return ctx;
        }

        #endregion

        #region 上下文配置

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
