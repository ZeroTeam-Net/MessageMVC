using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class EmptyStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.Close()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.End()
        {
            return Task.FromResult(true);
        }
    }
}