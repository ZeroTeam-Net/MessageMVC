using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.ZeroMQ;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class ZmqCaller
    {
        #region Properties

        static ulong nextId;

        /// <summary>
        ///     返回值
        /// </summary>
        internal ulong ID = ++nextId;

        /// <summary>
        ///     请求格式说明
        /// </summary>
        internal static readonly byte[] CallDescription =
        {
            0,(byte)ZeroByteCommand.General
        };


        /// <summary>
        /// 当前消息
        /// </summary>
        internal IInlineMessage Message;

        /// <summary>
        /// 最后一个返回值
        /// </summary>
        internal ZeroResult LastResult;

        #endregion

        #region Flow

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal async Task<IInlineMessage> CallAsync()
        {
            var req = GlobalContext.CurrentNoLazy?.Trace;
            var res = await CallZero();
            LogRecorder.MonitorTrace(() => $"result:{res.Result}");
            return res;
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <returns></returns>
        internal Task<IInlineMessage> CallZero()
        {
            var info = new TaskInfo
            {
                Caller = this,
                Start = DateTime.Now
            };
            if (!InporcFlow.Tasks.TryAdd(ID, info))
            {
                Message.State = MessageState.NetError;
                return Task.FromResult(Message);
            }
            using var message = new ZMessage
                {
                    new ZFrame(CallDescription)
                };
            bool res;
            ZError error;
            using (var socket = ZSocketEx.CreateOnceSocket(InporcFlow.InprocAddress, null, Message.ID.ToBytes(), ZSocketType.PAIR))
            {
                res = socket.Send(message, out error);
            }
            if (!res)
            {
                Message.State = MessageState.NetError;
                return Task.FromResult(Message);
            }
            info.TaskSource = new TaskCompletionSource<IInlineMessage>();
            return info.TaskSource.Task;
        }

        #endregion

    }
}

/*

        /// <summary>
        ///     消息状态
        /// </summary>
        internal MessageState MessageState
        {
            get
            {
                switch (LastResult.State)
                {
                    case ZeroOperatorStateType.Ok:
                        return MessageState.Success;
                    case ZeroOperatorStateType.Plan:
                    case ZeroOperatorStateType.Runing:
                    case ZeroOperatorStateType.VoteBye:
                    case ZeroOperatorStateType.Wecome:
                    case ZeroOperatorStateType.VoteSend:
                    case ZeroOperatorStateType.VoteWaiting:
                    case ZeroOperatorStateType.VoteStart:
                    case ZeroOperatorStateType.VoteEnd:
                        return MessageState.Accept;
                    case ZeroOperatorStateType.NetTimeOut:
                    case ZeroOperatorStateType.ExecTimeOut:
                        return MessageState.Cancel;
                    case ZeroOperatorStateType.NotFind:
                    case ZeroOperatorStateType.NoWorker:
                    case ZeroOperatorStateType.NotSupport:
                        return MessageState.NoSupper;
                    case ZeroOperatorStateType.FrameInvalid:
                    case ZeroOperatorStateType.ArgumentInvalid:
                        return MessageState.FormalError;
                    case ZeroOperatorStateType.Bug:
                    case ZeroOperatorStateType.Error:
                    case ZeroOperatorStateType.Failed:
                        return MessageState.Failed;
                    //case ZeroOperatorStateType.Pause:
                    //case ZeroOperatorStateType.DenyAccess:
                    //case ZeroOperatorStateType.LocalNoReady:
                    //case ZeroOperatorStateType.LocalZmqError:
                    //case ZeroOperatorStateType.LocalException:
                    //case ZeroOperatorStateType.LocalSendError:
                    //case ZeroOperatorStateType.LocalRecvError:
                    //    return MessageState.SendError;
                    //case ZeroOperatorStateType.NetError:
                    //case ZeroOperatorStateType.Unavailable:
                    //    return MessageState.NetError;
                    default:
                        return MessageState.NetError;
                }
            }
        }
*/
