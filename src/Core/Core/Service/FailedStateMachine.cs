using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class FailedStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> IStationStateMachine.Start()
        {
            return Control.DoStart();
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        Task<bool> IStationStateMachine.Close()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.End()
        {
            if (IsDisposed)
            {
                return Task.FromResult(false);
            }
            IsDisposed = true;
           return Control.DoEnd();
        }
    }
}