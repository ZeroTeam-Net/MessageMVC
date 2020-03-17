﻿namespace ZeroTeam.MessageMVC.ZeroServices.StateMachine
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
            ZeroApplication.OnObjectFailed(Station);
            return false;
        }
        /// <summary>
        ///     结束的处理
        /// </summary>
        bool IStationStateMachine.End()
        {
            ZeroApplication.OnObjectClose(Station);
            return false;
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