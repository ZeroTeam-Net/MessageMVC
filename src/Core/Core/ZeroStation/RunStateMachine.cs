﻿namespace ZeroTeam.MessageMVC.ZeroServices.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    internal class RunStateMachine : StateMachineBase, IStationStateMachine
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
        bool IStationStateMachine.Close()
        {
            if (IsDisposed)
            {
                ZeroApplication.OnObjectClose(Station);
                return false;
            }
            IsDisposed = true;
            return Station.Close();
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        bool IStationStateMachine.End()
        {
            return false;
        }
    }
}