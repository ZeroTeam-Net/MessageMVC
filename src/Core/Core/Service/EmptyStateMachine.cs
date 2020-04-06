using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Services.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class EmptyStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> IStationStateMachine.Start()
        {
            ZeroFlowControl.OnObjectFailed(Service);

            return Task.FromResult(false);
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.End()
        {
            Control.DoEnd();
            return Task.FromResult(true);
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        Task<bool> IStationStateMachine.Close()
        {
            return Task.FromResult(false);
        }
    }
}