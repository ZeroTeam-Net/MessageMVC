using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class CloseStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> IStationStateMachine.Start()
        {
            IsDisposed = false;
            return Control.DoStart();
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.Closing()
        {
            return Task.FromResult(true);
        }
    }
}