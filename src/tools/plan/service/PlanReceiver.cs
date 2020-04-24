﻿using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
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
        /// <summary>
        /// 构造
        /// </summary>
        public PlanReceiver() : base(nameof(PlanReceiver))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => null;

        #region IMessageReceiver

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
            _ = CheckReceipt(token);
            bool succes = true;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!succes)
                    {
                        succes = true;
                        await Task.Delay(PlanSystemOption.Instance.LoopIdleTime, token);
                        if (token.IsCancellationRequested)
                        {
                            return true;
                        }
                    }
                    while (wait > PlanSystemOption.Instance.MaxRunTask)
                    {
                        await Task.Delay(PlanSystemOption.Instance.LoopIdleTime, token);
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

                            //写入回执备查
                            item.Message.Trace??= TraceInfo.New(item.Message.ID);
                            item.Message.Trace.Context ??= new StaticContext();
                            item.Message.Trace.Context.Option ??= new System.Collections.Generic.Dictionary<string, string>();
                            item.Message.Trace.Context.Option["Receipt"] = "true";

                            _ = MessageProcessor.OnMessagePush(Service, item.Message, true, item);
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
            }
            PlanItem.logger.Information("计划任务轮询结束");
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        async Task IMessageReceiver.LoopComplete()
        {
            PlanItem.logger.Information("Plan queue loop complete....");
            while (wait > 0)
            {
                await Task.Delay(50);
            }
        }

        #endregion

        #region 计划执行

        private int wait = 0;

        private static async Task<bool> CheckResult(PlanItem item)
        {
            if (!PlanSystemOption.Instance.CheckPlanResult)
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
                case OperatorStatusCode.ReTry:
                case OperatorStatusCode.NoReady:
                case OperatorStatusCode.Unavailable:
                    item.RealInfo.exec_state = MessageState.Cancel;
                    break;
                case OperatorStatusCode.NoFind:
                    item.RealInfo.exec_state = MessageState.NonSupport;
                    break;
                case OperatorStatusCode.NetworkError:
                    item.RealInfo.exec_state = MessageState.NetworkError;
                    break;
                case OperatorStatusCode.BusinessException:
                case OperatorStatusCode.UnhandleException:
                    item.RealInfo.exec_state = MessageState.BusinessError;
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
            PlanItem.logger.Trace(() => $"计划任务执行结束,消息状态: {message.State}");

            //message.OfflineResult(Service.Serialize);全为远程调用,应该不需要

            if (message.State.IsEnd())
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
                if (message.State == MessageState.NetworkError)
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
        async Task CheckReceipt(CancellationToken token)
        {
            await Task.Yield();
            string pre = null;
            PlanItem.logger.Information("计划任务：回执检查已启动");
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var id = await RedisHelper.RPopLPushAsync(PlanItem.planErrorKey, PlanItem.planErrorKey);
                    if (id == null)
                    {
                        await Task.Delay(PlanSystemOption.idleTime,token);
                        continue;
                    }
                    if (id == pre)
                        await Task.Delay(PlanSystemOption.waitTime, token);//相同ID多次处理
                    else
                        pre = id;
                    var item = PlanItem.LoadMessage(id, false);
                    if (item == null)
                    {
                        await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
                        continue;
                    }
                    //BUG:返回值为空
                    var (msg, recri) = await MessagePoster.Post(MessageHelper.Simple(id,
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


                    if (message.State.IsEnd())
                    {
                        await item.CheckNextTime();
                    }
                    else
                    {
                        await item.ReTry();
                    }
                    await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);

                    await MessagePoster.Post(MessageHelper.Simple(id, ToolsOption.Instance.ReceiptService, "receipt/v1/remove", id));
                }
                catch (Exception ex)
                {
                    PlanItem.logger.Warning(() => $"检查回执发生异常.{ex.Message}");
                }
            }
            PlanItem.logger.Information("计划任务：回执检查已退出");
        }

        #endregion

    }
}