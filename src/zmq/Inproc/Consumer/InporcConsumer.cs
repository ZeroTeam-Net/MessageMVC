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
    public class InporcConsumer : IMessageConsumer
    {

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 服务
        /// </summary>
        public IService Service { get; set; }

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket socket;

        /// <summary>
        /// Pool对象
        /// </summary>
        private IZmqPool zmqPool;

        /// <summary>
        /// 初始化
        /// </summary>
        void INetTransfer.Initialize()
        {
            Name = "InporcConsumer";
        }

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> INetTransfer.LoopBegin()
        {
            socket = ZSocketEx.CreateServiceSocket(ZmqProxy.InprocAddress, null, ZSocketType.ROUTER);
            if (socket == null)
            {
                return Task.FromResult(false);
            }

            zmqPool = ZmqPool.CreateZmqPool();
            zmqPool.Sockets = new[] { socket };
            zmqPool.RePrepare(ZPollEvent.In);
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
            OnResult(ApiResultIoc.LocalExceptionJson, (ApiCallItem)tag, (byte)ZeroOperatorStateType.LocalException);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.OnResult(IMessageItem message, object tag)
        {
            OnResult(message.Result, (ApiCallItem)tag,
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
                _ = MessageProcess.OnMessagePush(Service, arg, item);
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
                if (item == null || !ZmqProxy.Instance.Tasks.TryGetValue(id, out var task))
                {
                    return;
                }
                ZmqProxy.Instance.Tasks.TryRemove(id, out _);
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
                if (item == null || !ZmqProxy.Instance.Tasks.TryGetValue(id, out var task))
                {
                    return;
                }
                ZmqProxy.Instance.Tasks.TryRemove(id, out _);
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

        #region Remote
#if Remote
        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            2,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.Requester,
            //ZeroFrameType.SerivceKey,
            ZeroFrameType.ResultEnd
        };
        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        void SendLayoutErrorResult(ApiCallItem item)
        {
            if (item == null)
            {
                return;
            }
            SendResult(new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(LayoutErrorFrame),
                new ZFrame(item.Requester),
                //new ZFrame(ServiceKey)
            });
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        void OnResult(string result, ApiCallItem item, byte state)
        {
            int i = 0;
            var des = new byte[9 + item.Originals.Count];
            des[i++] = (byte)(item.Originals.Count + (item.EndTag == ZeroFrameType.ResultFileEnd ? 6 : 5));
            des[i++] = state;
            des[i++] = ZeroFrameType.Requester;
            des[i++] = ZeroFrameType.RequestId;
            des[i++] = ZeroFrameType.CallId;
            des[i++] = ZeroFrameType.GlobalId;
            des[i++] = ZeroFrameType.ResultText;
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToZeroBytes(),
                item.RequestId.ToZeroBytes(),
                item.CallId.ToZeroBytes(),
                item.GlobalId.ToZeroBytes(),
                result?.ToZeroBytes()
            };
            if (item.EndTag == ZeroFrameType.ResultFileEnd)
            {
                des[i++] = ZeroFrameType.BinaryContent;
                msg.Add(item.Binary);
            }
            foreach (var org in item.Originals)
            {
                des[i++] = org.Key;
                msg.Add((org.Value));
            }
            //des[i++] = ZeroFrameType.SerivceKey;
            //msg.Add(ZMQProxy.ServiceKey);
            des[i] = item.EndTag > 0 ? item.EndTag : ZeroFrameType.ResultEnd;
            SendResult(new ZMessage(msg));
        }
        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool SendResult(ZMessage message)
        {
            using (message)
            {
                try
                {
                    if (socket.Send(message, out ZError error))
                    {
                        return true;
                    }
                    ZeroTrace.WriteError(Name, error.Text, error.Name);
                    LogRecorder.MonitorTrace(() => $"{Name}({socket.Endpoint}) : {error.Text}");
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "ApiStation.SendResult");
                    LogRecorder.MonitorTrace(() => $"Exception : {Name} : {e.Message}");
                }
            }
            return false;
        }
#endif
        #endregion
    }
}
