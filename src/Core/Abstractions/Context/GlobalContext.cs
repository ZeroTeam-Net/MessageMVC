using Agebull.Common.Ioc;
using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Context
{

    /// <summary>
    ///     全局上下文
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public static class GlobalContext
    {
        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static IUser User => Current.User;

        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static IZeroContext Current => IocScope.Dependency.TryGetDependency(IocHelper.Create<IZeroContext>);

        /// <summary>
        ///     当前线程的调用上下文(无懒构造)
        /// </summary>
        public static IZeroContext CurrentNoLazy => IocScope.Dependency.Dependency<IZeroContext>();

        static bool enableLinkTrace ;

        /// <summary>
        ///     启用调用链跟踪,默认为AppOption中的设置, 可通过远程传递而扩散
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public static bool EnableLinkTrace
        {
            set => enableLinkTrace = value;
            get => enableLinkTrace || CurrentNoLazy?.Option["EnableLinkTrace"] == "true";
        }

        /// <summary>
        /// 表示一个匿名用户
        /// </summary>
        public static IUser Anymouse { get; } = IocHelper.Create<IUser>();

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="context"></param>
        public static void SetContext(IZeroContext context)
        {
            if (null == context)
            {
                IocScope.Dependency.Remove< IZeroContext>();
            }
            else
            {
                IocScope.Dependency.Annex(context);
            }
        }

        /// <summary>
        ///     内部构造
        /// </summary>
        public static IZeroContext Reset() => IocScope.Dependency.Annex(IocHelper.Create<IZeroContext>());

        /// <summary>
        ///     置空并销毁当前上下文
        /// </summary>
        public static void SetEmpty() => IocScope.Dependency.Remove<IZeroContext>();

    }
}
