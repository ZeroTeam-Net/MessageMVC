using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Tools;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划接收器(轮询Redis列表)
    /// </summary>
    internal class PlanReceiver : MessageReceiverBase, IMessageReceiver
    {
        #region IMessageReceiver

        /// <summary>
        /// 初始化
        /// </summary>
        bool IMessageReceiver.Prepare()
        {
            PlanSystemOption.Option = ConfigurationManager.Get<PlanSystemOption>("MessageMVC:PlanTask");
            return PlanSystemOption.Option != null;
        }

        /// <summary>
        /// 开启
        /// </summary>
        async Task<bool> IMessageReceiver.LoopBegin()
        {
            PlanItem.logger.Information("Plan queue loog begin.");
            wait = 0;
            await PlanItem.Start();
            return true;
        }

        /// <summary>
        /// 开启
        /// </summary>
        async Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            CheckReceipt(token);
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
                        if (token.IsCancellationRequested)
                        {
                            return true;
                        }
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
                            PlanItem.logger.Trace(() => $"Plan message begin post.{item.Option.plan_id}");

                            Interlocked.Increment(ref wait);
                            item.Message.Reset();
                            await MessageProcessor.OnMessagePush(Service, item.Message, item);
                        }
                    }
                    catch (Exception ex)
                    {
                        PlanItem.logger.Warning(() => $"Plan queue loop error.{ex.Message}");
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
        async Task IMessageReceiver.LoopComplete()
        {
            while (wait > 0)
            {
                await Task.Delay(50);
            }
            PlanItem.logger.Information("Plan queue loop complete.");
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
            var result = ApiResultHelper.Helper.Deserialize(item.Message.Result);

            await item.SaveResult(result?.Trace?.Point ?? item.Message.Topic, item.Message.Result);
            if (result == null || result.Success)
            {
                return true;
            }
            switch (result.Code)
            {
                case DefaultErrorCode.ReTry:
                case DefaultErrorCode.NoReady:
                case DefaultErrorCode.Unavailable:
                    item.RealInfo.exec_state = MessageState.Cancel;
                    break;
                case DefaultErrorCode.NoFind:
                    item.RealInfo.exec_state = MessageState.NoSupper;
                    break;
                case DefaultErrorCode.NetworkError:
                    item.RealInfo.exec_state = MessageState.NetError;
                    break;
                case DefaultErrorCode.BusinessException:
                case DefaultErrorCode.UnhandleException:
                    item.RealInfo.exec_state = MessageState.Error;
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
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        async Task<bool> IMessageReceiver.OnResult(IInlineMessage message, object tag)
        {
            PlanItem item = (PlanItem)tag;
            Interlocked.Decrement(ref wait);
            PlanItem.logger.Trace(() => $"Plan post end.state {message.State}");

            message.OfflineResult(Service.Serialize);
            item.Message.CopyResult(message);
            if (message.State == MessageState.Success)
            {
                item.RealInfo.exec_state = item.Message.State;
                item.RealInfo.exec_end_time = PlanItem.NowTime();

                if (await CheckResult(item))
                {
                    item.RealInfo.retry_num = 0;
                    await item.CheckNextTime();
                }
            }
            else
            {
                item.RealInfo.error_num++;
                item.RealInfo.exec_state = item.Message.State;
                if (message.State == MessageState.NetError)
                {
                    await item.Error();
                }
                else
                {
                    await item.SaveResult(item.Message.Topic, item.Message.Result);
                    item.RealInfo.exec_end_time = PlanItem.NowTime();
                    await item.ReTry();
                }
            }
            return true;
        }
        #endregion

        #region 回执检测

        /// <summary>
        /// 回执检测
        /// </summary>
        async void CheckReceipt(CancellationToken token)
        {
            var rep = MessagePoster.GetService(ToolsOption.Instance.ReceiptService);
            if (rep == null)
            {
                PlanItem.logger.Error("回执服务未注册,无法处理异常回执确认");
                return;
            }
            await Task.Yield();
            try
            {
                string pre = null;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var id = await RedisHelper.RPopLPushAsync(PlanItem.planErrorKey, PlanItem.planErrorKey);
                        if (id == null)
                        {
                            await Task.Delay(PlanSystemOption.idleTime);
                            continue;
                        }
                        if (id == pre)
                            await Task.Delay(PlanSystemOption.waitTime);//相同ID多次处理
                        else
                            pre = id;
                        var item = PlanItem.LoadMessage(id, false);
                        if (item == null)
                        {
                            await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
                            continue;
                        }

                        var msg = await rep.Post(MessageHelper.Simple(id,
                            ToolsOption.Instance.ReceiptService, "receipt/v1/load", id));

                        if (msg.State != MessageState.Success || msg.State != MessageState.Failed)
                        {
                            //取不到,在30分钟后认为失败
                            if ((DateTime.Now - PlanItem.FromTime(item.RealInfo.exec_start_time)).TotalMinutes > 10)
                            {
                                await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
                                await item.ReTry();//远程错误,直接重试
                            }
                            continue;
                        }

                        var result = ApiResultHelper.Helper.Deserialize<InlineMessage>(msg.Result);
                        if (result == null || !result.Success || result.ResultData == null)
                        {
                            await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
                            await item.ReTry();//远程错误,直接重试
                            continue;
                        }
                        var message = result.ResultData;
                        await item.SaveResult(message.Topic, message.Result);

                        item.RealInfo.exec_state = item.Message.State;
                        item.RealInfo.exec_end_time = PlanItem.NowTime();
                        await item.SaveRealInfo();


                        if (message.State == MessageState.Success || message.State == MessageState.Failed)
                        {
                            await item.CheckNextTime();
                        }
                        else
                        {
                            await item.ReTry();
                        }
                        await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);

                        await rep.Post(MessageHelper.Simple(id, ToolsOption.Instance.ReceiptService, "receipt/v1/remove", id));
                    }
                    catch (Exception ex)
                    {
                        PlanItem.logger.Warning(() => $"Check receipt queue loop error.{ex.Message}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        #endregion

    }
}