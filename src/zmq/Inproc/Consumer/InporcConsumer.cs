using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    /// 表示进程内通讯
    /// </summary>
    public class InporcConsumer : NetTransferBase, IMessageConsumer
    {
        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket socket;

        /// <summary>
        /// Pool对象
        /// </summary>
        private IZmqPool zmqPool;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> INetTransfer.LoopBegin()
        {
            socket = ZSocketEx.CreateServiceSocket(ZmqFlowMiddleware.InprocAddress, null, ZSocketType.ROUTER);
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

        Task<bool> INetTransfer.Loop(CancellationToken token)
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
        Task INetTransfer.LoopComplete()
        {
            zmqPool.Dispose();
            socket.Dispose();
            return Task.CompletedTask;
        }
        /// <summary>
        /// 错误 
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.OnError(Exception exception, IMessageItem message, object tag)
        {
            if (tag is ApiCallItem item)
                OnResult(ApiResultIoc.LocalExceptionJson, item, (byte)ZeroOperatorStateType.LocalException);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.OnResult(IMessageItem message, object tag)
        {
            if (tag is ApiCallItem item)
                OnResult(message.Result, item,
                    (byte)(message.State == MessageState.Success
                            ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Bug));
            return Task.CompletedTask;
        }

        #region ZMQ

        private void Listen()
        {
            if (!zmqPool.Poll() || !zmqPool.CheckIn(0, out var message))
            {
                return;
            }

            if (!ApiCallItem.Unpack(message, out var item) || string.IsNullOrWhiteSpace(item.ApiName))
            {
                SendLayoutErrorResult(item);
                return;
            }

            var arg = new MessageItem
            {
                Title = item.ApiName,
                Topic = item.Station,
                Content = item.Argument,
                Context = item.Context
            };
            try
            {
                _ = MessageProcessor.OnMessagePush(Service, arg, item);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                OnResult(ApiResultIoc.LocalExceptionJson, item, (byte)ZeroOperatorStateType.LocalException);
            }
        }


        #endregion

        #region Task

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private void SendLayoutErrorResult(ApiCallItem item)
        {
            try
            {
                var id = long.Parse(item.Caller.FromUtf8Bytes().Trim('"'));
                if (item == null || !ZmqFlowMiddleware.Instance.Tasks.TryGetValue(id, out var task))
                {
                    return;
                }
                ZmqFlowMiddleware.Instance.Tasks.TryRemove(id, out _);
                task.TaskSource.TrySetResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.FrameInvalid,
                    Result = ApiResultIoc.ArgumentErrorJson
                });
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        private void OnResult(string result, ApiCallItem item, byte state)
        {
            try
            {
                var id = long.Parse(item.Caller.FromUtf8Bytes().Trim('"'));
                if (item == null || !ZmqFlowMiddleware.Instance.Tasks.TryGetValue(id, out var task))
                {
                    return;
                }
                ZmqFlowMiddleware.Instance.Tasks.TryRemove(id, out _);
                task.TaskSource.TrySetResult(new ZeroResult
                {
                    State = (ZeroOperatorStateType)state,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
        }

        #endregion

    }
}
