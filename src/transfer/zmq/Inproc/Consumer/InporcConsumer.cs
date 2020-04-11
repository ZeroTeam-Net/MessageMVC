using Agebull.Common.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.ZeroMQ;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    /// 表示进程内通讯
    /// </summary>
    public class InporcConsumer : MessageReceiverBase, IMessageConsumer
    {
        #region IMessageConsumer

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            socket = ZSocketEx.CreateServiceSocket(InporcFlow.InprocAddress, null, ZSocketType.ROUTER);
            if (socket == null)
            {
                return Task.FromResult(false);
            }

            zmqPool = ZmqPool.CreateZmqPool();
            zmqPool.Sockets = new[] { socket };
            zmqPool.RePrepare(ZPollEvent.In);
            State = StationStateType.Run;
            return Task.FromResult(true);
        }

        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Listen();
                }
                catch (OperationCanceledException)//取消为正常操作,不记录
                {
                    return Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex, "InporcConsumer.Loop");
                }
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.LoopComplete()
        {
            zmqPool.Dispose();
            socket.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.OnResult(IInlineMessage message, object tag)
        {
            var callItem = tag as ZmqCaller;
            if (!InporcFlow.Tasks.TryGetValue(callItem.ID, out var task))
            {
                return Task.FromResult(false);
            }
            InporcFlow.Tasks.TryRemove(callItem.ID, out _);
            var res = task.TaskSource.TrySetResult(message);
            return Task.FromResult(res);
        }

        #endregion

        #region ZMQ

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket socket;

        /// <summary>
        /// Pool对象
        /// </summary>
        private IZmqPool zmqPool;

        private void Listen()
        {
            if (!zmqPool.Poll() || !zmqPool.CheckIn(0, out var zMessage))
            {
                return;
            }

            ApiCallItem.Unpack(zMessage, out var callItem);

            var id = ulong.Parse(callItem.Caller.FromUtf8Bytes());
            InporcFlow.Tasks.TryGetValue(id, out var task);
            _ = MessageProcessor.OnMessagePush(Service, task.Caller.Message, task);
        }

        #endregion

    }
}

/*

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        internal void CheckStateResult()
        {
            IApiResult apiResult;
            switch (LastResult.State)
            {
                case ZeroOperatorStateType.Ok:
                    Message.Result = LastResult.Result;
                    return;
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                case ZeroOperatorStateType.NotSupport:
                    apiResult = ApiResultHelper.Ioc.NoFind;
                    break;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    apiResult = ApiResultHelper.Ioc.NoReady;
                    break;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    apiResult = ApiResultHelper.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.LocalException:
                    apiResult = ApiResultHelper.Ioc.LocalException;
                    break;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    apiResult = ApiResultHelper.Ioc.Error(DefaultErrorCode.Success, LastResult.State.ToString());
                    break;
                case ZeroOperatorStateType.Error:
                    apiResult = ApiResultHelper.Ioc.InnerError;
                    break;
                case ZeroOperatorStateType.Unavailable:
                    apiResult = ApiResultHelper.Ioc.Unavailable;
                    break;
                case ZeroOperatorStateType.NetTimeOut:
                    apiResult = ApiResultHelper.Ioc.NetTimeOut;
                    break;
                case ZeroOperatorStateType.ExecTimeOut:
                    apiResult = ApiResultHelper.Ioc.ExecTimeOut;
                    break;
                case ZeroOperatorStateType.ArgumentInvalid:
                    apiResult = ApiResultHelper.Ioc.ArgumentError;
                    break;
                case ZeroOperatorStateType.FrameInvalid:
                case ZeroOperatorStateType.NetError:
                    apiResult = ApiResultHelper.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.Bug:
                case ZeroOperatorStateType.Failed:
                    apiResult = ApiResultHelper.Ioc.LogicalError;
                    break;
                case ZeroOperatorStateType.Pause:
                    apiResult = ApiResultHelper.Ioc.Pause;
                    break;
                case ZeroOperatorStateType.DenyAccess:
                    apiResult = ApiResultHelper.Ioc.DenyAccess;
                    break;
                default:
                    apiResult = ApiResultHelper.Ioc.RemoteEmptyError;
                    break;
            }

            Message.Result ??= JsonHelper.SerializeObject(apiResult);

        }
*/
