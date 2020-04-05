using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System.Threading;
using ZeroTeam.MessageMVC.Messages;

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
        static AsyncLocal<IZeroContext> Local { get; } = new AsyncLocal<IZeroContext>();

        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static IZeroContext Current
        {
            get
            {
                if (Local.Value != null)
                {
                    return Local.Value;
                }
                return Reset();
            }
        }

        /// <summary>
        ///     启用调用链跟踪,默认为AppOption中的设置, 可通过远程传递而扩散
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public static bool EnableLinkTrace => CurrentNoLazy?.Option.EnableLinkTrace ?? ZeroAppOption.Instance.EnableLinkTrace;

        /// <summary>
        ///     当前线程的调用上下文(无懒构造)
        /// </summary>
        public static IZeroContext CurrentNoLazy => Local?.Value;

        /// <summary>
        ///     内部构造
        /// </summary>
        public static IZeroContext Reset()
        {
            Local.Value = IocHelper.Create<IZeroContext>();
            if (Local.Value != null)
            {
                return Local.Value;
            }
            IocHelper.AddScoped<IZeroContext, ZeroContext>();
            return Local.Value = IocHelper.Create<IZeroContext>();

        }

        /// <summary>
        ///     置空并销毁当前上下文
        /// </summary>
        public static void SetEmpty()
        {
            Local.Value = null;
        }
        /// <summary>
        /// 表示一个匿名用户
        /// </summary>
        public static IUser Anymouse { get; } = new UserInfo
        {
            UserId = -1,
            NickName = "Anymouse"
        };

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="context"></param>
        public static void SetContext(IZeroContext context)
        {
            if (null == context)
            {
                Local.Value = null;
            }
            else if (Local.Value != context)
            {
                Local.Value = context;
            }
        }
    }
}
