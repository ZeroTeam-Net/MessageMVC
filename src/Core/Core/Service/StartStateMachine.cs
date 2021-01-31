using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class StartStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> IStationStateMachine.Start()
        {
            IsDisposed = false;
            return Control.DoStart();
        }
    }
}