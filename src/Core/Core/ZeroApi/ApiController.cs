using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// ZeroApi控制器基类
    /// </summary>
    public abstract class ApiController : IApiControler
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public IUser UserInfo => GlobalContext.Customer;

        /// <summary>
        /// 调用标识
        /// </summary>
        public string RequestId => GlobalContext.CurrentNoLazy?.Request?.RequestId;

        /// <summary>
        /// HTTP调用时的UserAgent
        /// </summary>
        public string UserAgent => GlobalContext.CurrentNoLazy?.Request?.UserAgent;

        private IMessageItem _message;

        /// <summary>
        /// 原始调用帧消息
        /// </summary>
        public IMessageItem Message
        {
            get
            {
                if (!ZeroFlowControl.Config.EnableGlobalContext)
                    return null;
                return _message ?? (_message = GlobalContext.Current.DependencyObjects.Dependency<IMessageItem>());
            }
        }
    }
}