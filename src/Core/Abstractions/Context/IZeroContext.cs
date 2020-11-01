using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;
using ActionTask = System.Threading.Tasks.TaskCompletionSource<(ZeroTeam.MessageMVC.Messages.MessageState state, object result)>;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///     全局上下文
    /// </summary>
    public interface IZeroContext
    {
        /// <summary>
        ///     是否延迟处理
        /// </summary>
        bool IsDelay { get; set; }

        /// <summary>
        /// 依赖范围
        /// </summary>
        IDisposable  DependencyScope { get; set; }

        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        IUser User { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        TraceInfo Trace { get; }

        /// <summary>
        /// 全局状态
        /// </summary>
        ContextStatus Status { get; set; }

        /// <summary>
        /// 上下文配置
        /// </summary>
        Dictionary<string, string> Option { get; set; }

        /// <summary>
        /// 当前消息
        /// </summary>
        IInlineMessage Message { get; set; }

        /// <summary>
        /// 当前任务，用于提前返回
        /// </summary>
        ActionTask Task { get; set; }
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
