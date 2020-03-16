using System;

namespace ZeroTeam.MessageMVC.ZeroServices.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class StationStateMachineBase : IDisposable
    {
        /// <summary>
        /// 站点
        /// </summary>
        public IService Station { get; set; }


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