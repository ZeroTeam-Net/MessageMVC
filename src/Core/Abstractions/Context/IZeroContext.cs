using Agebull.EntityModel.Common;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///     全局上下文
    /// </summary>
    public interface IZeroContext
    {
        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        IUser User { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        TraceInfo Trace { get; set; }

        /// <summary>
        /// 全局状态
        /// </summary>
        ContextStatus Status { get; set; }

        /// <summary>
        /// 上下文配置
        /// </summary>
        ContextOption Option { get; set; }

        /// <summary>
        /// 当前消息
        /// </summary>
        IMessageItem Message { get; set; }

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        DependencyObjects DependencyObjects { get; }

    }
}

/*
/// <summary>
///     令牌
/// </summary>
string Token { get; }

/// <summary>
///     当前调用的组织信息
/// </summary>
IOrganizational Organizational { get; }

/// <summary>
///     最后状态(当前时间)
/// </summary>
IOperatorStatus LastStatus { get; }


/// <summary>
///     当前调用的客户的角色信息
/// </summary>
Dictionary<string,IRole> Role { get; }
*/
