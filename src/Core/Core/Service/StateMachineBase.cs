using System;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class StateMachineBase : IDisposable
    {
        /// <summary>
        /// 站点
        /// </summary>
        public ZeroService Service { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        public IStateMachineControl Control => Service;


        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; protected set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }
    }
}