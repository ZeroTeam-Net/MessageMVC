using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class ZeroRPCCaller
    {
        #region Properties

        static private long id;
        /// <summary>
        /// 取得Id
        /// </summary>
        /// <returns></returns>
        static long GetId()
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
                return;
            IApiResult apiResult;
            switch (State)
            {
                case ZeroOperatorStateType.Ok:
                    apiResult = ApiResultIoc.Ioc.Ok;
                    break;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    apiResult = ApiResultIoc.Ioc.NoReady;
                    break;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    apiResult = ApiResultIoc.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.LocalException:
                    apiResult = ApiResultIoc.Ioc.LocalException;
                    break;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    apiResult = ApiResultIoc.Ioc.Error(ErrorCode.Success, State.Text());
                    break;
                case ZeroOperatorStateType.Error:
                    apiResult = ApiResultIoc.Ioc.InnerError;
                    break;
                case ZeroOperatorStateType.Unavailable:
                    apiResult = ApiResultIoc.Ioc.Unavailable;
                    break;
                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    apiResult = ApiResultIoc.Ioc.NoFind;
                    break;
                case ZeroOperatorStateType.ArgumentInvalid:
                    apiResult = ApiResultIoc.Ioc.ArgumentError;
                    break;
                case ZeroOperatorStateType.NetTimeOut:
                    apiResult = ApiResultIoc.Ioc.NetTimeOut;
                    break;
                case ZeroOperatorStateType.ExecTimeOut:
                    apiResult = ApiResultIoc.Ioc.ExecTimeOut;
                    break;
                case ZeroOperatorStateType.FrameInvalid:
                    apiResult = ApiResultIoc.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.NetError:

                    apiResult = ApiResultIoc.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    apiResult = ApiResultIoc.Ioc.LogicalError;
                    break;
                case ZeroOperatorStateType.Pause:
                    apiResult = ApiResultIoc.Ioc.Pause;
                    break;
                case ZeroOperatorStateType.DenyAccess:
                    apiResult = ApiResultIoc.Ioc.DenyAccess;
                    break;
                default:
                    apiResult = ApiResultIoc.Ioc.RemoteEmptyError;
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
            LastResult = await ZeroRPCProxy.Instance.CallZero(this,
                 CallDescription,
                 Commmand.ToZeroBytes(),
                 Argument.ToZeroBytes(),
                 ExtendArgument.ToZeroBytes(),
                 GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                 Name.ToString().ToZeroBytes(),
                 "".ToZeroBytes(),// GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                 GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
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