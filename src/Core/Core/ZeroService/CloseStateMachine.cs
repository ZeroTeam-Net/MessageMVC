namespace ZeroTeam.MessageMVC.ZeroApis.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class CloseStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        bool IStationStateMachine.Start()
        {
            return Control.DoStart();
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        bool IStationStateMachine.Close()
        {
            return true;
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        bool IStationStateMachine.End()
        {
            Control.DoEnd();
            return true;
        }
    }
}