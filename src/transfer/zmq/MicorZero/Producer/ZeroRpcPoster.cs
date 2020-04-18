using Agebull.Common;
using Agebull.Common.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     ZMQ生产者
    /// </summary>
    public class ZeroRPCPoster : MessagePostBase, IMessagePoster
    {
        #region Properties

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(ZeroRPCPoster);

        StationStateType state;

        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType IMessagePoster.State { get => state; set => state = value; }

        /// <summary>
        /// 实例
        /// </summary>
        public static ZeroRPCPoster Instance = new ZeroRPCPoster();

        /// <summary>
        /// 构造
        /// </summary>
        private ZeroRPCPoster()
        {
            Instance = this;
        }

        #endregion

        #region IMessagePoster

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            LogRecorder.MonitorDetails("[ZeroRPCPoster.Post] 开始发送");
            var req = GlobalContext.CurrentNoLazy?.Trace;
            var offline = new MessageItem
            {
                Trace = message.Trace?.Copy(),
                State = message.State
            };
            if (message.Trace != null)
            {
                offline.Trace = new TraceInfo
                {
                    //TraceId = message.Trace.TraceId,
                    Start = message.Trace.Start,
                    //LocalId = message.Trace.LocalId,
                    //LocalApp = message.Trace.LocalApp,
                    //LocalMachine = message.Trace.LocalMachine,
                    //CallId = message.Trace.CallId,
                    CallApp = message.Trace.CallApp,
                    //CallMachine = message.Trace.CallMachine,
                    Headers = message.Trace.Headers,
                    Token = message.Trace.Token,
                    //Context= message.Trace.Context,
                    Level = message.Trace.Level + 1
                };
            }
            message.ArgumentOffline();

            return await ZeroPostProxy.Instance.CallZero(message,
                 CallDescription,
                 message.ApiName.ToBytes(),//Command
                 message.Argument.ToBytes(),//Argument
                 SmartSerializer.SerializeMessage(offline).ToBytes(),//TextContent
                 req == null ? ByteHelper.EmptyBytes : req.LocalId.ToBytes(),//Context
                 req == null ? ByteHelper.EmptyBytes : req.TraceId.ToBytes(),//RequestId
                 message.ID.ToBytes(),//CallId
                 ZeroAppOption.Instance.TraceName.ToBytes(),//Requester
                 GlobalContext.CurrentNoLazy.ToJsonBytes());

        }

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.TextContent,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.RequestId,
            ZeroFrameType.Requester,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        internal static IOperatorStatus GetOperatorStatus(ZeroOperatorStateType state)
        {
            switch (state)
            {
                case ZeroOperatorStateType.Ok:
                    return ApiResultHelper.Helper.Ok;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    return ApiResultHelper.Helper.NoReady;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    return ApiResultHelper.Helper.NetworkError;
                case ZeroOperatorStateType.LocalException:
                    return ApiResultHelper.Helper.BusinessException;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    return ApiResultHelper.Helper.State(OperatorStatusCode.Success, state.Text());
                case ZeroOperatorStateType.Error:
                    return ApiResultHelper.Helper.BusinessException;
                case ZeroOperatorStateType.Unavailable:
                    return ApiResultHelper.Helper.Unavailable;
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    return ApiResultHelper.Helper.NoFind;
                case ZeroOperatorStateType.ArgumentInvalid:
                    return ApiResultHelper.Helper.ArgumentError;
                case ZeroOperatorStateType.NetTimeOut:
                    return ApiResultHelper.Helper.NetTimeOut;
                case ZeroOperatorStateType.ExecTimeOut:
                    return ApiResultHelper.Helper.ExecTimeOut;
                case ZeroOperatorStateType.FrameInvalid:
                    return ApiResultHelper.Helper.NetworkError;
                case ZeroOperatorStateType.NetworkError:
                    return ApiResultHelper.Helper.NetworkError;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    return ApiResultHelper.Helper.BusinessError;
                case ZeroOperatorStateType.Pause:
                    return ApiResultHelper.Helper.Pause;
                case ZeroOperatorStateType.DenyAccess:
                    return ApiResultHelper.Helper.DenyAccess;
                default:
                    return ApiResultHelper.Helper.NonSupport;
            }
        }

        /// <summary>
        ///     消息状态
        /// </summary>
        internal static MessageState GetMessageState(ZeroOperatorStateType state)
        {
            switch (state)
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
                case ZeroOperatorStateType.NonSupport:
                    return MessageState.NonSupport;
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
                //    return   MessageState.SendError;
                //case ZeroOperatorStateType.NetworkError:
                //case ZeroOperatorStateType.Unavailable:
                //    return   MessageState.NetworkError;
                default:
                    return MessageState.NetworkError;
            }
        }

        #endregion
    }
}

/*

        string IMessagePoster.Producer(string topic, string title, string content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
            return client.Result;
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
        void IMessagePoster.Producer<TArg>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
        }
        TRes IMessagePoster.Producer<TRes>(string topic, string title)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title
            };
            client.CallCommand();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }


        async Task<string> IMessagePoster.ProducerAsync(string topic, string title, string content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            await client.CallCommandAsync();
            return client.Result;
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            await client.CallCommandAsync();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
        Task IMessagePoster.ProducerAsync<TArg>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            return client.CallCommandAsync();
        }
        async Task<TRes> IMessagePoster.ProducerAsync<TRes>(string topic, string title)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title
            };
            await client.CallCommandAsync();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
*/
