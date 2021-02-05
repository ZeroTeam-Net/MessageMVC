using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class RunStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     关闭的处理
        /// </summary>
        Task<bool> IStationStateMachine.Closing()
        {
            return Control.DoClosing();
        }
    }
}