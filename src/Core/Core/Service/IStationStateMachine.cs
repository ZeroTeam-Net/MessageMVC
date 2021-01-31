using System;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 站点状态机
    /// </summary>
    public interface IStationStateMachine : IDisposable
    {
        /// <summary>
        /// 站点
        /// </summary>
        ZeroService Service { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        IStateMachineControl Control { get; }

        /// <summary>
        /// 是否已析构
        /// </summary>
        bool IsDisposed { get; set; }

        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> Start()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        Task<bool> Closing()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        Task<bool> Close()
        {
            if (Service == null)
                return Task.FromResult(true);
            if (IsDisposed)
            {
                ZeroFlowControl.OnObjectClose(Service);
                return Task.FromResult(false);
            }
            IsDisposed = true;
            return Control.DoClose();
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> End() => Control?.DoEnd();
    }
}