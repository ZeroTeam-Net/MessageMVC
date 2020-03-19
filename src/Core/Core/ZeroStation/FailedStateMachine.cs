﻿namespace ZeroTeam.MessageMVC.ZeroServices.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class FailedStateMachine : StateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        bool IStationStateMachine.Start()
        {
            ZeroApplication.OnObjectFailed(Service);
            return false;
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        bool IStationStateMachine.Close()
        {
            ZeroApplication.OnObjectClose(Service);
            return false;
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        bool IStationStateMachine.End()
        {
            if (IsDisposed)
                return false;
            IsDisposed = true;
            Control.DoEnd();
            return true;
        }
    }
}