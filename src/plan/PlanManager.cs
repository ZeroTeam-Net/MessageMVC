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
    public class PlanManager : IFlowMiddleware
    {
        #region IFlowMiddleware

        string IFlowMiddleware.RealName => "PlanManager";

        int IFlowMiddleware.Level => -1;

        private CancellationTokenSource tokenSource;

        /// <summary>
        ///     初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
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
        void IFlowMiddleware.Start()
        {
            tokenSource = new CancellationTokenSource();
            PlanLoop();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void IFlowMiddleware.Close()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }

        #endregion
        #region 计划执行

        private int wait = 0;

        /// <summary>
        /// 开启
        /// </summary>
        private async void PlanLoop()
        {
            await Task.Yield();

            PlanItem.Start();
            bool succes = true;
            while (!tokenSource.IsCancellationRequested)
            {
                if (!succes)
                {
                    succes = true;
                    await Task.Delay(PlanSystemOption.Option.LoopIdleTime);
                    if (tokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                }
                while (wait > PlanSystemOption.Option.MaxRunTask)
                {
                    await Task.Delay(PlanSystemOption.Option.LoopIdleTime);
                }
                try
                {
                    var item = PlanItem.Pop();
                    if (!item.state)
                    {
                        continue;
                    }
                    if (item.item == null)
                    {
                        succes = false;
                        continue;
                    }
                    Interlocked.Increment(ref wait);
                    //await ExecMessage(item);
                    ExecMessage(item.item);
                }
                catch// (Exception)
                {
                    succes = false;
                    continue;
                }
            }
            while (wait > 0)
            {
                await Task.Delay(50);
            }
        }

        private async void ExecMessage(PlanItem item)
        {
            await Task.Yield();
            using (MonitorScope.CreateScope($"Plan({item.Option.plan_id}) => {item.Message.Topic}/{item.Message.Title}"))
            {
                string res;
                try
                {
                    res = await MessageProducer.ProducerAsync(item.Message.Topic, item.Message.Title, item.Message.Content);
                    LogRecorder.MonitorTrace(res);

                    item.RealInfo.exec_end_time = PlanItem.Time();
                    item.RealInfo.real_repet++;
                    if (!CheckResult(item, res))
                    {
                        item.RealInfo.retry_repet = 0;
                        item.RealInfo.exec_state = MessageState.Success;
                        item.CheckNextTime();
                    }
                }
                catch (Exception ex)
                {
                    LogRecorder.Trace("Exception:{0}", ex.Message);
                    LogRecorder.Exception(ex);
                    item.RealInfo.exec_state = MessageState.Exception;
                    item.ReTry();
                }
            }
            Interlocked.Decrement(ref wait);
        }

        private static bool CheckResult(PlanItem item, string res)
        {
            if (!PlanSystemOption.Option.CheckPlanResult)
            {
                item.save_message_result(item.Message.Topic, res);
                return true;
            }
            var result = JsonHelper.TryDeserializeObject<ApiResult>(res);

            item.save_message_result(result?.Status?.Point ?? item.Message.Topic, res);
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
            item.ReTry();
            return false;
        }
        #endregion
    }

}