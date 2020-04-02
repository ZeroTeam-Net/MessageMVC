using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划管理器
    /// </summary>
    internal class PlanTransfer : INetTransfer
    {

        #region IFlowMiddleware

        public IService Service { get; set; }
        public string Name { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        void INetTransfer.Initialize()
        {
            ConfigurationManager.RegistOnChange(
                () =>
                {
                    PlanSystemOption.Option = ConfigurationManager.Get<PlanSystemOption>("PlanTask")
                                            ?? new PlanSystemOption();
                },
                true);
        }

        /// <summary>
        /// 开启
        /// </summary>
        async Task<bool> INetTransfer.LoopBegin()
        {
            wait = 0;
            await PlanItem.Start();
            return true;
        }

        /// <summary>
        /// 开启
        /// </summary>
        async Task<bool> INetTransfer.Loop(CancellationToken token)
        {
            bool succes = true;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!succes)
                    {
                        succes = true;
                        await Task.Delay(PlanSystemOption.Option.LoopIdleTime);
                        if (token.IsCancellationRequested)
                        {
                            return true;
                        }
                    }
                    while (wait > PlanSystemOption.Option.MaxRunTask)
                    {
                        await Task.Delay(PlanSystemOption.Option.LoopIdleTime);
                    }
                    try
                    {
                        var (state, item) = await PlanItem.Pop();
                        if (!state)
                        {
                            succes = false;
                        }
                        else if (item != null)
                        {
                            Interlocked.Increment(ref wait);
                            //await ExecMessage(item);
                            item.Message.Flush();
                            await MessageProcess.OnMessagePush(Service, item.Message, item);
                        }
                    }
                    catch
                    {
                        succes = false;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return true;
            }
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        async Task INetTransfer.LoopComplete()
        {
            while (wait > 0)
            {
                await Task.Delay(50);
            }
        }

        public void Dispose()
        {
        }
        #endregion

        #region 计划执行

        private int wait = 0;

        private static async Task<bool> CheckResult(PlanItem item)
        {
            if (!PlanSystemOption.Option.CheckPlanResult)
            {
                await item.SaveResult(item.Message.Topic, item.Message.Result);
                return true;
            }
            ApiResult result = JsonHelper.TryDeserializeObject<ApiResult>(item.Message.Result);

            await item.SaveResult(result?.Status?.Point ?? item.Message.Topic, item.Message.Result);
            if (result == null || result.Success)
            {
                return true;
            }
            switch (result.Status.Code)
            {
                case ErrorCode.ReTry:
                case ErrorCode.NoReady:
                case ErrorCode.Unavailable:
                    item.RealInfo.exec_state = MessageState.Cancel;
                    break;
                case ErrorCode.NoFind:
                    item.RealInfo.exec_state = MessageState.NoSupper;
                    break;
                case ErrorCode.RemoteError:
                case ErrorCode.LocalException:
                    item.RealInfo.exec_state = MessageState.Exception;
                    break;
                default:
                    item.RealInfo.exec_state = MessageState.Failed;
                    break;
            }
            item.RealInfo.error_num++;
            await item.ReTry();
            return false;
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        async Task INetTransfer.OnResult(IMessageItem message, object tag)
        {
            PlanItem item = (PlanItem)tag;

            item.RealInfo.exec_state = item.Message.State;
            item.RealInfo.exec_end_time = PlanItem.NowTime();

            if (await CheckResult(item))
            {
                item.RealInfo.retry_num = 0;
                await item.CheckNextTime();
            }
        }

        /// <summary>
        /// 错误 
        /// </summary>
        /// <returns></returns>
        async Task INetTransfer.OnError(Exception exception, IMessageItem message, object tag)
        {
            PlanItem item = (PlanItem)tag;
            await item.SaveResult(item.Message.Topic, item.Message.Result);
            item.RealInfo.error_num++;
            item.RealInfo.exec_state = item.Message.State;
            item.RealInfo.exec_end_time = PlanItem.NowTime();
            await item.ReTry();
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.OnCallEnd(IMessageItem message, object tag)
        {
            Interlocked.Decrement(ref wait);
            return Task.CompletedTask;
        }
        #endregion
    }

}