using Agebull.Common;
using Agebull.Common.Logging;
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

        private static long id;

        /// <summary>
        /// 取得Id
        /// </summary>
        /// <returns></returns>
        private static long GetId()
        {
            return Interlocked.Increment(ref id);
        }
        /// <summary>
        ///     调用时使用的名称
        /// </summary>
        internal long Name = GetId();

        /// <summary>
        ///     返回值
        /// </summary>
        internal byte ResultType;

        /// <summary>
        ///     返回值
        /// </summary>
        internal string Result;

        /// <summary>
        ///     返回值
        /// </summary>
        internal byte[] Binary;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal string Station;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal StationConfig Config;

        /// <summary>
        ///     上下文内容（透传方式）
        /// </summary>
        internal string ContextJson;

        /// <summary>
        ///     标题
        /// </summary>
        internal string Title;

        /// <summary>
        ///     调用命令
        /// </summary>
        internal string Commmand;

        /// <summary>
        ///     参数
        /// </summary>
        internal string Argument;

        /// <summary>
        ///     扩展参数
        /// </summary>
        internal string ExtendArgument;

        /// <summary>
        ///     结果状态
        /// </summary>
        internal ZeroOperatorStateType State;

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
        internal bool Call()
        {
            var task = CallApi();
            task.Wait();
            return task.Result;
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal Task<bool> CallAsync()
        {
            return CallApi();
        }

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        internal void CheckStateResult()
        {
            if (Result != null)
            {
                return;
            }

            IApiResult apiResult;
            switch (State)
            {
                case ZeroOperatorStateType.Ok:
                    apiResult = ApiResultHelper.Ioc.Ok;
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
                    apiResult = ApiResultHelper.Ioc.Error(DefaultErrorCode.Success, State.Text());
                    break;
                case ZeroOperatorStateType.Error:
                    apiResult = ApiResultHelper.Ioc.InnerError;
                    break;
                case ZeroOperatorStateType.Unavailable:
                    apiResult = ApiResultHelper.Ioc.Unavailable;
                    break;
                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    apiResult = ApiResultHelper.Ioc.NoFind;
                    break;
                case ZeroOperatorStateType.ArgumentInvalid:
                    apiResult = ApiResultHelper.Ioc.ArgumentError;
                    break;
                case ZeroOperatorStateType.NetTimeOut:
                    apiResult = ApiResultHelper.Ioc.NetTimeOut;
                    break;
                case ZeroOperatorStateType.ExecTimeOut:
                    apiResult = ApiResultHelper.Ioc.ExecTimeOut;
                    break;
                case ZeroOperatorStateType.FrameInvalid:
                    apiResult = ApiResultHelper.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.NetError:

                    apiResult = ApiResultHelper.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
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
            //if (LastResult != null && LastResult.InteractiveSuccess)
            //{
            //    if (!LastResult.TryGetString(ZeroFrameType.Responser, out var point))
            //        point = "zero_center";
            //    apiResult.Status.Point = point;
            //}
            Result = JsonHelper.SerializeObject(apiResult);
        }

        /// <summary>
        ///     消息状态
        /// </summary>
        internal MessageState MessageState
        {
            get
            {
                if (Result != null)
                {
                    return MessageState.Success;
                }
                switch (State)
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
        #endregion

        #region Socket

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription =
        {
            9,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Requester,
            ZeroFrameType.Responser,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };


        private async Task<bool> CallApi()
        {
            var req = GlobalContext.CurrentNoLazy?.Trace;
            LastResult = await ZeroPostProxy.Instance.CallZero(this,
                 CallDescription,
                 Commmand.ToZeroBytes(),
                 Argument.ToZeroBytes(),
                 ExtendArgument.ToZeroBytes(),
                 req == null ? JsonHelper.EmptyBytes : req.TraceId.ToZeroBytes(),
                 Name.ToString().ToZeroBytes(),
                 JsonHelper.EmptyBytes,// GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                 req == null ? JsonHelper.EmptyBytes : req.LocalId.ToZeroBytes(),
                 ContextJson.ToZeroBytes() ?? GlobalContext.CurrentNoLazy.ToZeroBytes());

            State = LastResult.State;
            Result = LastResult.Result;
            Binary = LastResult.Binary;
            ResultType = LastResult.ResultType;
            LogRecorder.MonitorTrace(() => $"result:{Result}");
            return LastResult.InteractiveSuccess;
        }
        #endregion

    }
}