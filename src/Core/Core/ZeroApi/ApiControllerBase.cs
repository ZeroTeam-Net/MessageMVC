using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// ZeroApi控制器基类
    /// </summary>
    public class ApiControllerBase : IApiControler
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public IUser UserInfo => GlobalContext.Customer;

        /// <summary>
        /// 调用者（机器名）
        /// </summary>
        public string Caller => GlobalContext.ServiceName;

        /// <summary>
        /// 调用标识
        /// </summary>
        public string RequestId => GlobalContext.RequestInfo.RequestId;

        /// <summary>
        /// HTTP调用时的UserAgent
        /// </summary>
        public string UserAgent => GlobalContext.RequestInfo.UserAgent;

        IMessageItem _message;

        /// <summary>
        /// 原始调用帧消息
        /// </summary>
        public IMessageItem Message => _message ?? (_message = GlobalContext.Current.DependencyObjects.Dependency<IMessageItem>());

    }
}