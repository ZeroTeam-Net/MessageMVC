using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.ZeroApis.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class RunStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> IStationStateMachine.Start()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.Close()
        {
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
        Task<bool> IStationStateMachine.End()
        {
            Control.DoEnd();
            return Task.FromResult(true);
        }
    }

}