using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ActionTask = System.Threading.Tasks.TaskCompletionSource<(ZeroTeam.MessageMVC.Messages.MessageState state, object result)>;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///     全局上下文
    /// </summary>
    public interface IZeroContext : IScopeContext
    {
        /// <summary>
        ///     跟踪信息
        /// </summary>
        TraceInfo TraceInfo { get; }

        /// <summary>
        ///     当前用户
        /// </summary>
        IUser User { get; set; }

        /// <summary>
        /// 全局状态
        /// </summary>
        ContextStatus Status { get; }

        /// <summary>
        /// 上下文配置
        /// </summary>
        Dictionary<string, string> Option { get; }

        /// <summary>
        /// 当前消息
        /// </summary>
        IInlineMessage Message { get; set; }

        /// <summary>
        /// 请求的任务，用于提前返回
        /// </summary>
        ActionTask RequestTask { get; set; }

        /// <summary>
        /// 当前执行的任务，用于正确等待
        /// </summary>
        TaskCompletionSource<bool> ActionTask { get; set; }

        /// <summary>
        /// 提前设置返回值，这会导致方法不中断，而框架则提前返回到消息调用处
        /// </summary>
        void SetResult(MessageState state, object result);
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
