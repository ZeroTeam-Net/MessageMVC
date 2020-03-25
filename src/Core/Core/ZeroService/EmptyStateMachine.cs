namespace ZeroTeam.MessageMVC.ZeroApis.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class EmptyStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        bool IStationStateMachine.Start()
        {
            ZeroFlowControl.OnObjectFailed(Service);
            return false;
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        bool IStationStateMachine.End()
        {
            Control.DoEnd();
            return true;
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        bool IStationStateMachine.Close()
        {
            return false;
        }
    }
}