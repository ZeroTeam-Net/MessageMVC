using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        static ulong nextId;

        /// <summary>
        ///     返回值
        /// </summary>
        internal ulong ID = ++nextId;


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
        internal async Task<bool> CallAsync()
        {
            var req = GlobalContext.CurrentNoLazy?.Trace;
            var result = await ZeroPostProxy.Instance.CallZero(this,
                 CallDescription,
                 Message.ApiName.ToBytes(),
                 Message.Argument.ToBytes(),
                 req == null ? ByteHelper.EmptyBytes : req.TraceId.ToBytes(),
                 ID.ToString().ToBytes(),
                 req == null ? ByteHelper.EmptyBytes : req.LocalId.ToBytes(),
                 GlobalContext.CurrentNoLazy.ToJsonBytes());

            return result == ZeroOperatorStateType.Ok;
        }

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        internal void CheckResult(string json, ZeroOperatorStateType state)
        {
            LogRecorder.Trace(() => $"OnRemoteResult : {state}\n{state}");
            if (json != null)
            {
                Message.Result = json;
                return;
            }

            IApiResult apiResult;
            switch (state)
            {
                case ZeroOperatorStateType.Ok:
                    apiResult = ApiResultHelper.Helper.Ok;
                    break;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    apiResult = ApiResultHelper.Helper.NoReady;
                    break;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    apiResult = ApiResultHelper.Helper.NetworkError;
                    break;
                case ZeroOperatorStateType.LocalException:
                    apiResult = ApiResultHelper.Helper.BusinessException;
                    break;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    apiResult = ApiResultHelper.Helper.Error(DefaultErrorCode.Success, state.Text());
                    break;
                case ZeroOperatorStateType.Error:
                    apiResult = ApiResultHelper.Helper.BusinessException;
                    break;
                case ZeroOperatorStateType.Unavailable:
                    apiResult = ApiResultHelper.Helper.Unavailable;
                    break;
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    apiResult = ApiResultHelper.Helper.NoFind;
                    break;
                case ZeroOperatorStateType.ArgumentInvalid:
                    apiResult = ApiResultHelper.Helper.ArgumentError;
                    break;
                case ZeroOperatorStateType.NetTimeOut:
                    apiResult = ApiResultHelper.Helper.NetTimeOut;
                    break;
                case ZeroOperatorStateType.ExecTimeOut:
                    apiResult = ApiResultHelper.Helper.ExecTimeOut;
                    break;
                case ZeroOperatorStateType.FrameInvalid:
                    apiResult = ApiResultHelper.Helper.NetworkError;
                    break;
                case ZeroOperatorStateType.NetError:

                    apiResult = ApiResultHelper.Helper.NetworkError;
                    break;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    apiResult = ApiResultHelper.Helper.BusinessError;
                    break;
                case ZeroOperatorStateType.Pause:
                    apiResult = ApiResultHelper.Helper.Pause;
                    break;
                case ZeroOperatorStateType.DenyAccess:
                    apiResult = ApiResultHelper.Helper.DenyAccess;
                    break;
                default:
                    apiResult = ApiResultHelper.Helper.NotSupport;
                    break;
            }
            //if (LastResult != null && LastResult.InteractiveSuccess)
            //{
            //    if (!LastResult.TryGetString(ZeroFrameType.Responser, out var point))
            //        point = "zero_center";
            //    apiResult.Status.Point = point;
            //}
            Message.Result = JsonHelper.SerializeObject(apiResult);
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
                case ZeroOperatorStateType.NotSupport:
                    Message.State = MessageState.NoSupper;
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
                //case ZeroOperatorStateType.NetError:
                //case ZeroOperatorStateType.Unavailable:
                //    Message.State =  MessageState.NetError;
                default:
                    Message.State = MessageState.NetError;
                    break;
            }
        }

        #endregion

        #region Socket

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.RequestId,
            ZeroFrameType.Requester,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };


        #endregion

    }
}