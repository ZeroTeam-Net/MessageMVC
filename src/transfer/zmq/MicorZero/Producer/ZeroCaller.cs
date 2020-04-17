using Agebull.Common;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class ZeroCaller
    {
        #region Properties

        /// <summary>
        /// 当前消息
        /// </summary>
        internal IInlineMessage Message;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal StationConfig Config;

        #endregion

        #region Flow

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal async Task<MessageResult> CallAsync()
        {
            var req = GlobalContext.CurrentNoLazy?.Trace;
            var offline = new MessageItem
            {
                Trace = Message.Trace,
                State = Message.State
            };
            if(offline.Trace != null)
            {
                offline.Trace.Context = null;
                offline.Trace.TraceId = null;
                offline.Trace.CallMachine = null;
                offline.Trace.CallId = null;
                offline.Trace.CallMachine = null;
            }

            return await ZeroPostProxy.Instance.CallZero(this,
                 CallDescription,
                 Message.ApiName.ToBytes(),//Command
                 Message.Argument.ToBytes(),//Argument
                 offline.ToJson().ToBytes(),//TextContent
                 req == null ? ByteHelper.EmptyBytes : req.LocalId.ToBytes(),//Context
                 req == null ? ByteHelper.EmptyBytes : req.TraceId.ToBytes(),//RequestId
                 Message.ID.ToBytes(),//CallId
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
        internal IOperatorStatus GetOperatorStatus(ZeroOperatorStateType state)
        {
            switch (state)
            {
                case ZeroOperatorStateType.Ok:
                    return  ApiResultHelper.Helper.Ok;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    return  ApiResultHelper.Helper.NoReady;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    return  ApiResultHelper.Helper.NetworkError;
                case ZeroOperatorStateType.LocalException:
                    return  ApiResultHelper.Helper.BusinessException;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    return  ApiResultHelper.Helper.Error(DefaultErrorCode.Success, state.Text());
                case ZeroOperatorStateType.Error:
                    return  ApiResultHelper.Helper.BusinessException;
                case ZeroOperatorStateType.Unavailable:
                    return  ApiResultHelper.Helper.Unavailable;
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    return  ApiResultHelper.Helper.NoFind;
                case ZeroOperatorStateType.ArgumentInvalid:
                    return  ApiResultHelper.Helper.ArgumentError;
                case ZeroOperatorStateType.NetTimeOut:
                    return  ApiResultHelper.Helper.NetTimeOut;
                case ZeroOperatorStateType.ExecTimeOut:
                    return  ApiResultHelper.Helper.ExecTimeOut;
                case ZeroOperatorStateType.FrameInvalid:
                    return  ApiResultHelper.Helper.NetworkError;
                case ZeroOperatorStateType.NetworkError:
                    return  ApiResultHelper.Helper.NetworkError;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    return  ApiResultHelper.Helper.BusinessError;
                case ZeroOperatorStateType.Pause:
                    return  ApiResultHelper.Helper.Pause;
                case ZeroOperatorStateType.DenyAccess:
                    return  ApiResultHelper.Helper.DenyAccess;
                default:
                    return  ApiResultHelper.Helper.NonSupport;
            }
        }

        /// <summary>
        ///     消息状态
        /// </summary>
        internal void CheckState(ZeroOperatorStateType state)
        {
            switch (state)
            {
                case ZeroOperatorStateType.Ok:
                    Message.State = MessageState.Success;
                    break;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    Message.State = MessageState.Accept;
                    break;
                case ZeroOperatorStateType.NetTimeOut:
                case ZeroOperatorStateType.ExecTimeOut:
                    Message.State = MessageState.Cancel;
                    break;
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                case ZeroOperatorStateType.NonSupport:
                    Message.State = MessageState.NonSupport;
                    break;
                case ZeroOperatorStateType.FrameInvalid:
                case ZeroOperatorStateType.ArgumentInvalid:
                    Message.State = MessageState.FormalError;
                    break;
                case ZeroOperatorStateType.Bug:
                case ZeroOperatorStateType.Error:
                case ZeroOperatorStateType.Failed:
                    Message.State = MessageState.Failed;
                    break;
                //case ZeroOperatorStateType.Pause:
                //case ZeroOperatorStateType.DenyAccess:
                //case ZeroOperatorStateType.LocalNoReady:
                //case ZeroOperatorStateType.LocalZmqError:
                //case ZeroOperatorStateType.LocalException:
                //case ZeroOperatorStateType.LocalSendError:
                //case ZeroOperatorStateType.LocalRecvError:
                //    Message.State =  MessageState.SendError;
                //case ZeroOperatorStateType.NetworkError:
                //case ZeroOperatorStateType.Unavailable:
                //    Message.State =  MessageState.NetworkError;
                default:
                    Message.State = MessageState.NetworkError;
                    break;
            }
        }

        #endregion

    }
}