using Agebull.Common.Ioc;
using System;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///     范围上下文
    /// </summary>
    public interface IScopeContext : IDictionaryTransfer
    {
        /// <summary>
        /// 范围数据
        /// </summary>
        ScopeAttachData ScopeData { get; set; }

        /// <summary>
        /// 依赖范围内数据
        /// </summary>
        IDisposable Scope { get; set; }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        IScopeContext Clone();
    }
}