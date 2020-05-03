using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks.BusinessLogic;
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
        /// 启动
        /// </summary>
        async Task<bool> IMessageReceiver.LoopBegin()
        {
            PlanItem.logger.Information("计划任务轮询开始");
            wait = 0;
            await PlanItem.Start();
            return true;
        }

        /// <summary>
        /// 启动
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
                    (bool state, PlanItem item) task;
                    try
                    {
                        task = await PlanItem.Pop();
                    }
                    catch (Exception ex)
                    {
                        PlanItem.logger.Exception(ex, "计划任务轮询出错");
                        succes = false;
                        continue;
                    }
                    if (!task.state)
                    {
                        succes = false;
                        continue;
                    }
                    if (task.item == null)
                    {
                        continue;
                    }
                    Interlocked.Increment(ref wait);
                    _ = MessageProcessor.OnMessagePush(Service, task.item.Message, true, task.item);
                }
            }
            catch (TaskCanceledException)
            {
            }
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        async Task IMessageReceiver.LoopComplete()
        {
            PlanItem.logger.Information("计划任务轮询正在关闭中....");
            while (wait > 0)
            {
                await Task.Delay(50);
            }
            PlanItem.logger.Information("计划任务轮询结束");
        }

        #endregion

        #region 计划结果

        private int wait = 0;

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        async Task<bool> IMessageReceiver.OnResult(IInlineMessage message, object tag)
        {
            Interlocked.Decrement(ref wait);

            await CheckRemoteResult(message, (PlanItem)tag);

            return true;
        }

        private static async Task CheckRemoteResult(IInlineMessage message, PlanItem item)
        {
            if (message.State.IsEnd())
            {
                item.RealInfo.ExecState = item.Message.State;
                item.RealInfo.ExecEndTime = PlanItem.NowTime();

                item.Monitor.Trace($"执行完成,消息状态: {message.State}");
                if (await CheckResult(item))
                {
                    item.RealInfo.RetryNum = 0;
                    await item.CheckNextTime();
                }
            }
            else
            {
                item.RealInfo.ErrorNum++;
                item.RealInfo.ExecState = item.Message.State;
                if (message.State == MessageState.NetworkError)
                {
                    item.Monitor.Trace("执行时遇到网络错误,将通过获取回执决定后续结果");
                    await item.WaitReceipt(PlanMessageState.error);
                }
                else if (message.State == MessageState.AsyncQueue)
                {
                    item.Monitor.Trace("正在异步执行,将通过获取回执决定后续结果");
                    await item.WaitReceipt(PlanMessageState.waiting);
                }
                else
                {
                    item.RealInfo.ExecEndTime = PlanItem.NowTime();
                    int time = await item.ReTry();
                    item.Monitor.Trace(time > 0
                        ? $"执行状态{message.State}，重试执行"
                        : $"执行状态{message.State}，超过最大重试次数，异常关闭");
                }
            }
            await TaskExecutionBusinessLogic.SaveToDatabase(item, item.Message);
        }

        private static async Task<bool> CheckResult(PlanItem item)
        {
            if (!PlanSystemOption.Instance.CheckPlanResult)
            {
                return true;
            }
            var result = ApiResultHelper.Helper.Deserialize(item.Message.Result);

            if (result == null || result.Success)
            {
                return true;
            }
            switch (result.Code)
            {
                case OperatorStatusCode.ReTry:
                case OperatorStatusCode.NoReady:
                case OperatorStatusCode.Unavailable:
                    item.RealInfo.ExecState = MessageState.Cancel;
                    break;
                case OperatorStatusCode.NoFind:
                    item.RealInfo.ExecState = MessageState.NonSupport;
                    break;
                case OperatorStatusCode.NetworkError:
                    item.RealInfo.ExecState = MessageState.NetworkError;
                    break;
                case OperatorStatusCode.BusinessException:
                case OperatorStatusCode.UnhandleException:
                    item.RealInfo.ExecState = MessageState.BusinessError;
                    break;
                default:
                    item.RealInfo.ExecState = MessageState.Failed;
                    break;
            }
            item.RealInfo.ErrorNum++;
            await item.ReTry();
            return false;
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
                PlanItem item;
                string id;
                try
                {
                    id = await RedisHelper.RPopLPushAsync(PlanItem.planErrorKey, PlanItem.planErrorKey);
                    if (id == null)
                    {
                        await Task.Delay(PlanSystemOption.idleTime, token);
                        continue;
                    }
                    if (id == pre)
                        await Task.Delay(PlanSystemOption.waitTime, token);//相同ID多次处理
                    else
                        pre = id;
                    item = await PlanItem.LoadPlan(id, false, true);
                    if (item == null)
                    {
                        await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    PlanItem.logger.Warning(() => $"获取回执队列异常.{ex.Message}");
                    continue;
                }
                item.Monitor.BeginStep("回执检查");
                if (!await LoadReceipt(id, item))
                {
                    item.Monitor.EndStep();
                    await item.WaitReceipt(PlanMessageState.error);
                }
            }
            PlanItem.logger.Information("计划任务：回执检查已退出");
        }

        private async Task<bool> LoadReceipt(string id, PlanItem item)
        {
            (IInlineMessage msg, MessageState state) re;
            try
            {
                re = await MessagePoster.Post(MessageHelper.Simple(
                   item.Option.PlanId,
                   ToolsOption.Instance.ReceiptService,
                   "receipt/v1/load",
                   item.Option.PlanId));
            }
            catch (Exception ex)
            {
                item.Monitor.Trace($"提取回执发生异常.{ex.Message}");
                return false;
            }
            if (re.state != MessageState.Success || re.state != MessageState.Failed)
            {
                await CheckResultTimeOut(id, item);
                return false;
            }

            var result = ApiResultHelper.Helper.Deserialize<InlineMessage>(re.msg.Result);
            if (result == null || !result.Success || result.ResultData == null)
            {
                await CheckResultTimeOut(id, item);
                return false;
            }
            item.Monitor.Trace($"已获取回执.执行状态为{result.ResultData.State}");
            item.Monitor.EndStep();
            try
            {
                await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
                await MessagePoster.CallAsync(ToolsOption.Instance.ReceiptService, "receipt/v1/remove", new { id });

            }
            catch (Exception ex)
            {
                item.Monitor.Trace($"处理回执发生异常.{ex.Message}");
            }
            await CheckRemoteResult(result.ResultData, item);
            return true;
        }

        private async Task<bool> CheckResultTimeOut(string id, PlanItem item)
        {
            if ((DateTime.Now - PlanItem.FromTime(item.RealInfo.ExecStartTime)).TotalSeconds < item.Option.CheckResultTime)
            {
                item.Monitor.Trace($"获取回执失败.{state}");
                return false;
            }
            item.Monitor.Trace("超过10分钟未取得回执，系统假设远程未正常处理.任务进入重试");
            await RedisHelper.LRemAsync(PlanItem.planErrorKey, 0, id);
            await item.ReTry();//远程错误,直接重试
            return true;
        }

        #endregion

    }
}