using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        internal async Task Monitor()
        {
            await Task.Yield();
            MonitorStateMachine.Logger.Information("Zero center in monitor...");

            List<string> subs = new List<string>();
            if (ZeroRpcFlow.Config.CanRaiseEvent != true)
            {
                subs.Add("system");
                subs.Add("station");
            }
            bool success = true;
            while (ZeroRpcFlow.IsAlive)
            {
                if (!success)
                {
                    await Task.Delay(1000);
                }
                var socket = ZSocketEx.CreateSubSocket(
                    ZeroRpcFlow.Config.Master.MonitorAddress,
                    ZeroRpcFlow.Config.Master.ServiceKey.ToBytes(),
                    ZSocketHelper.CreateIdentity(false, "Monitor"), subs);
                if (socket == null)
                {
                    success = false;
                    continue;
                }
                DateTime failed = DateTime.MinValue;
                using var poll = ZmqPool.CreateZmqPool();
                poll.Prepare(ZPollEvent.In, socket);
                while (ZeroRpcFlow.IsAlive)
                {
                    if (poll.Poll())
                    {
                        if (!poll.CheckIn(0, out var message))
                        {
                            continue;
                        }

                        failed = DateTime.MinValue;
                        if (PublishItem.Unpack(message, out var item) && MonitorStateMachine.StateMachine != null)
                        {
                            await MonitorStateMachine.StateMachine.OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
                        }
                    }
                    else if (failed == DateTime.MinValue)
                    {
                        failed = DateTime.Now;
                    }
                    else if ((DateTime.Now - failed).TotalSeconds > 10)
                    {
                        //超时，连接重置
                        MonitorStateMachine.Logger.Error("Zero center event monitor failed,there was no message for a long time");
                        ZeroRpcFlow.ZeroCenterState = ZeroCenterState.Failed;
                        ZeroRpcFlow.RaiseEvent(ZeroNetEventType.CenterSystemStop, true);
                        break;
                    }
                }
            }
            MonitorStateMachine.Logger.Information("Zero center monitor stoped!");
            TaskEndSem.Release();
        }
    }
}