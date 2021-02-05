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
        async Task<bool> IStationStateMachine.Start()
        {
            if (!IsDisposed)
                await Control.DoClose();
            IsDisposed = false;
            return await Control.DoStart();
        }
    }
}