using System;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///     全局上下文
    /// </summary>
    public interface IGlobalContext : IDisposable
    {
        /// <summary>
        ///     当前登录的用户ID
        /// </summary>
        long LoginUserId { get; }
        
        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        IUser User { get; }

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        RequestInfo Request { get; }

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
    }
}