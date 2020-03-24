using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;

namespace ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    internal class ZeroEventMonitor
    {
        internal readonly SemaphoreSlim TaskEndSem = new SemaphoreSlim(0);

        /// <summary>
        /// 等待正常结束
        /// </summary>
        internal async Task WaitMe()
        {
            await TaskEndSem.WaitAsync();
        }

        /// <summary>
        ///     进入系统分布式消息侦听处理
        /// </summary>
        internal void Start()
        {
            _ = Monitor();
        }

        /// <summary>
        ///     进入系统分布式消息侦听处理
        /// </summary>
        async Task Monitor()
        {
            ZeroTrace.SystemLog("Zero center in monitor...");
            List<string> subs = new List<string>();
            if (MicroZeroApplication.Config.CanRaiseEvent != true)
            {
                subs.Add("system");
                subs.Add("station");
            }
            while (MicroZeroApplication.IsAlive)
            {
                DateTime failed = DateTime.MinValue;
                using var poll = ZmqPool.CreateZmqPool();
                var socket = ZSocketEx.CreateSubSocket(
                    MicroZeroApplication.Config.Master.MonitorAddress, 
                    MicroZeroApplication.Config.Master.ServiceKey.ToZeroBytes(),
                    ZSocketHelper.CreateIdentity(false, "Monitor"), subs);

                poll.Prepare(ZPollEvent.In, socket);
                while (MicroZeroApplication.IsAlive)
                {
                    if (poll.Poll())
                    {
                        if (!poll.CheckIn(0, out var message))
                            continue;
                        failed = DateTime.MinValue;
                        if (PublishItem.Unpack(message, out var item) && MonitorStateMachine.StateMachine != null)
                        {
                            await MonitorStateMachine.StateMachine.OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
                        }
                    }
                    else if (failed == DateTime.MinValue)
                        failed = DateTime.Now;
                    else if ((DateTime.Now - failed).TotalMinutes > 1)
                    {
                        //超时，连接重置
                        ZeroTrace.WriteError("Zero center event monitor failed,there was no message for a long time");
                        MicroZeroApplication.ZeroCenterState = ZeroCenterState.Failed;
                        await MicroZeroApplication.OnZeroCenterClose();
                        Thread.Sleep(500);
                        break;
                    }
                }
            }
            ZeroTrace.SystemLog("Zero center monitor stoped!");
            TaskEndSem.Release();
        }
    }
}