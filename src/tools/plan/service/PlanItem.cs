using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks.BusinessLogic;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    public class PlanItem
    {

        #region 系统配置与常量

        internal static ILogger<PlanItem> logger = DependencyHelper.LoggerFactory.CreateLogger<PlanItem>();

        private int planAutoRemoveTime => PlanSystemOption.Instance.CloseTimeout;

        internal const string OptionKey = "opt";
        internal const string RealKey = "rea";
        internal const string MonitorKey = "mon";
        internal const string MessageKey = "msg";

        private string Key => $"msg:plan:{Option.PlanId}";

        internal const string planIdKey = "msg:plan:id";
        internal const string planSetKey = "plan:time:set";
        internal const string planDoingKey = "plan:time:do";
        internal const string planErrorKey = "plan:time:err";
        internal const string planPauseKey = "plan:time:pau";

        internal const int second2ms = 1000;
        internal const int minute2ms = 60 * 1000;
        internal const int hour2ms = 60 * 60 * 1000;
        internal const int day2ms = 24 * 60 * 60 * 1000;

        /// <summary>
        /// 基准时间 2020-3-12
        /// </summary>
        public static readonly DateTime baseTime = new DateTime(2020, 3, 12);

        /// <summary>
        /// 当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long NowTime() => (long)(DateTime.Now - baseTime).TotalMilliseconds;

        /// <summary>
        /// 时间戳转为时间
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static DateTime FromTime(long ms) => baseTime.AddMilliseconds(ms);

        /// <summary>
        /// 时间转为时间戳
        /// </summary>
        /// <param name="time">不得小于基准时间 2020-3-12</param>
        /// <returns></returns>
        public static long ToTime(DateTime time) => (long)(time - baseTime).TotalMilliseconds;

        #endregion

        #region 计划对象

        /// <summary>
        /// 计划配置
        /// </summary>
        public PlanOption Option { get; set; }

        /// <summary>
        /// 跟踪
        /// </summary>
        public MonitorItem Monitor { get; set; }

        /// <summary>
        /// 实时信息
        /// </summary>
        public PlanRealInfo RealInfo { get; set; }

        /// <summary>
        /// 计划对象
        /// </summary>
        public IInlineMessage Message { get; set; }

        #endregion

        #region 状态处理

        /// <summary>
        /// 首次保存
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FirstSave()
        {
            var now = NowTime();
            Option.PlanId = Message.ID;
            Option.AddTime = now;
            if (Option.PlanTime <= 0)
            {
                Option.PlanTime = now;
            }

            RealInfo = new PlanRealInfo
            {
                PlanTime = Option.PlanTime
            };
            var tm = Option.PlanTime;
            switch (Option.PlanType)
            {
                case PlanTimeType.second:
                case PlanTimeType.minute:
                case PlanTimeType.hour:
                case PlanTimeType.day:
                case PlanTimeType.time:
                    //无效设置,自动放弃
                    if (Option.PlanValue <= 0)
                    {
                        return false;
                    }

                    break;
                case PlanTimeType.week:
                    //无效设置,自动放弃
                    if (Option.PlanValue < 0 || Option.PlanValue > 6)
                    {
                        return false;
                    }

                    Option.PlanTime = (long)FromTime(tm).TimeOfDay.TotalMilliseconds;
                    RealInfo.PlanTime = ToTime(tm > day2ms ? FromTime(Option.PlanTime).Date : DateTime.Today);
                    break;
                case PlanTimeType.month:
                    Option.PlanTime = (long)FromTime(tm).TimeOfDay.TotalMilliseconds;
                    RealInfo.PlanTime = ToTime(tm > day2ms ? FromTime(Option.PlanTime).Date : DateTime.Today);
                    break;
                default:
                    return false;
            }

            if (Option.RetrySet <= 0)
                Option.RetrySet = PlanSystemOption.Instance.RetryCount;
            if (Option.CheckResultTime <= 0)
                Option.CheckResultTime = PlanSystemOption.Instance.CheckResultTime;


            //写入回执备查
            Message.Trace ??= TraceInfo.New(Message.ID);
            Message.Trace.Context ??= new StaticContext();
            Message.Trace.Context.Option ??= new Dictionary<string, string>();
            Message.Trace.Context.Option["Receipt"] = "true";
            Option.DataId = await TaskInfoBusinessLogic.SaveToDatabase(this);
#if !UNIT_TEST
            await RedisHelper.HSetAsync(Key, OptionKey, SmartSerializer.ToInnerString(Option));
            await RedisHelper.HSetAsync(Key, RealKey, SmartSerializer.ToInnerString(RealInfo));
            await RedisHelper.HSetAsync(Key, MessageKey, SmartSerializer.SerializeMessage(Message));
#endif
            return true;
        }

        /// <summary>
        /// 设置为等待回执
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitReceipt(PlanMessageState state)
        {
            RealInfo.PlanState = state;
#if !UNIT_TEST
            await RedisHelper.HDelAsync(planDoingKey, Option.PlanId);
            await RedisHelper.LPushAsync(planErrorKey, Option.PlanId);
            await RedisHelper.HSetAsync(Key, RealKey, SmartSerializer.ToInnerString(RealInfo));
            await RedisHelper.ZRemAsync(planSetKey, Option.PlanId);
            await RedisHelper.HSetAsync(Key, MonitorKey, SmartSerializer.ToInnerString(Monitor));

#endif
            return true;
        }

        /// <summary>
        /// 重试
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReTry()
        {
            await RedisHelper.HDelAsync(Key, MonitorKey);
            RealInfo.PlanState = PlanMessageState.retry;
            if (++RealInfo.RetryNum > Option.RetrySet)
            {
                await Close();
                return -1;
            }

            var timeout = PlanSystemOption.Instance.RetryDelay.Length < RealInfo.RetryNum
                ? PlanSystemOption.Instance.RetryDelay[RealInfo.RetryNum]
                : PlanSystemOption.Instance.RetryDelay[^1];

            await JoinQueue(NowTime() + timeout);

            return timeout;
        }

        /// <summary>
        /// 恢复执行
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Reset()
        {
            await RedisHelper.LRemAsync(planErrorKey, 0, Option.PlanId);
            await JoinQueue(RealInfo.PlanTime);
            return true;
        }

        /// <summary>
        /// 暂停执行
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Pause()
        {
            logger.Trace(() => $"Plan is pause.{Option.PlanId}");
            RealInfo.PlanState = PlanMessageState.pause;

#if !UNIT_TEST
            await RedisHelper.HDelAsync(planDoingKey, Option.PlanId);
            await RedisHelper.LPushAsync(planPauseKey, Option.PlanId);
            await RedisHelper.HSetAsync(Key, RealKey, SmartSerializer.ToInnerString(RealInfo));
            await RedisHelper.ZRemAsync(planSetKey, Option.PlanId);
#endif
            //plan_dispatcher.instance->zero_event(zero_net_event.event_plan_pause, this);
            return true;
        }

        /// <summary>
        /// 设置跳过
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public async Task Skip(int set)
        {
            if (set < 0)
            {
                Option.SkipSet = -1;
            }
            else
            {
                Option.SkipSet = set;
            }
#if !UNIT_TEST
            await RedisHelper.HSetAsync(Key, OptionKey, SmartSerializer.ToInnerString(Option));
#endif
            if (Option.SkipSet < RealInfo.SkipNum)
            {
                await CheckNextTime();
            }
            else
            {
                await Close();
            }
        }

        /// <summary>
        /// 关闭一个消息
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Close()
        {
            RealInfo.PlanState = PlanMessageState.close;
            await Remove(Option.PlanId);
            await TaskInfoBusinessLogic.OnClose(this);
            return true;
        }

        /// <summary>
        /// 删除一个消息
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Remove(string id)
        {
#if !UNIT_TEST
            await RedisHelper.HDelAsync(planDoingKey, id);
            await RedisHelper.ZRemAsync(planSetKey, id);
            await RedisHelper.LRemAsync(planErrorKey, 0, id);
            await RedisHelper.LRemAsync(planPauseKey, 0, id);
            await RedisHelper.DelAsync($"msg:plan:{id}");
#endif
            return true;
        }
        #endregion

        #region 执行当前

        /// <summary>
        /// 启动,
        /// </summary>
        public static async Task Start()
        {
            try
            {
                var ids = await RedisHelper.HGetAllAsync<long>(planDoingKey);
                if (ids != null)
                {
                    logger.Information(() => $"Coutinue .{ids}");
                    foreach (var id in ids)
                    {
                        await RedisHelper.ZAddAsync(planSetKey, (id.Value, id.Key));
                    }
                }
                await RedisHelper.DelAsync(planDoingKey);
            }
            catch (Exception ex)
            {
                logger.Warning(() => $"Start error.{ex.Message}");
            }
        }

        /// <summary>
        /// 载入可执行的第一条内容
        /// </summary>
        /// <returns></returns>
        public static async Task<(bool state, PlanItem item)> Pop()
        {
            var now = NowTime();
            var msgs = await RedisHelper.ZRangeByScoreWithScoresAsync(planSetKey, 0, now, 1);
            var (member, score) = msgs.FirstOrDefault();
            if (member == null)
            {
                return (false, null);
            }

            var item = await LoadPlan(member, false, false);
            if (item == null || item.Option == null || item.RealInfo == null)
            {
                logger.Trace(() => $"读取任务失败.{member}");
                await Remove(member);
                return (true, null);
            }
            //设置为跳过的,直接计算下一次
            if (item.RealInfo.PlanState == PlanMessageState.skip)
            {
                ++item.RealInfo.SkipNum;
                logger.Trace(() => $"任务跳过本次执行.{member},执行次数 {item.RealInfo.SkipNum}");
                await item.CheckNextTime();
                return (true, null);
            }
            var msg = await RedisHelper.HGetAsync(item.Key, MessageKey);
            item.Message = SmartSerializer.FromInnerString<InlineMessage>(msg);
            if (item.Message == null)
            {
                logger.Trace(() => $"读取消息失败.{member}");
                await Remove(member);
                return (true, null);
            }

            await RedisHelper.HSetAsync(planDoingKey, member, score);
            await RedisHelper.ZRemAsync(planSetKey, member);

            item.RealInfo.PlanState = PlanMessageState.execute;
            item.RealInfo.ExecNum++;
            item.RealInfo.ExecStartTime = NowTime();
            item.RealInfo.ExecState = MessageState.None;
            await RedisHelper.HSetAsync(item.Key, RealKey, SmartSerializer.ToInnerString(item.RealInfo));

            return (true, item);
        }
        #endregion

        #region 执行时间计算

        /// <summary>
        /// 保存下一次执行时间
        /// </summary>
        /// <returns></returns>
        public async Task CheckNextTime()
        {
            await RedisHelper.HDelAsync(Key, MonitorKey);
            await DoCheckNext();
        }

        /// <summary>
        /// 计算下一次执行时间
        /// </summary>
        /// <returns></returns>
        public async Task DoCheckNext()
        {
            if ((Option.PlanRepet > 0 && Option.PlanRepet <= RealInfo.ExecNum + RealInfo.SkipNum) ||
                Option.PlanRepet == 0 && RealInfo.ExecNum > 0)
            {
                await Close();
                return;
            }
            switch (Option.PlanType)
            {
                case PlanTimeType.time:
                    await CheckTime();
                    return;
                case PlanTimeType.second:
                    await CheckDelay(Option.PlanValue * second2ms);
                    return;
                case PlanTimeType.minute:
                    await CheckDelay(Option.PlanValue * minute2ms);
                    return;
                case PlanTimeType.hour:
                    await CheckDelay(Option.PlanValue * hour2ms);
                    return;
                case PlanTimeType.day:
                    await CheckDelay(Option.PlanValue * day2ms);
                    return;
                case PlanTimeType.week:
                    await CheckWeek();
                    return;
                case PlanTimeType.month:
                    await CheckMonth();
                    return;
            }
        }


        /// <summary>
        /// 检查时间
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckTime()
        {
            var now = NowTime();
            if (RealInfo.SuccessNum > 0)
            {
                await Close();
            }
            else if (RealInfo.PlanTime <= 0)
            {
                await JoinQueue(now);
            }
            else if (Option.QueuePassBy)
            {
                await JoinQueue(RealInfo.PlanTime);
            }
            else if (now >= RealInfo.PlanTime)
            {
                RealInfo.SkipNum = 1;
                await Close();
            }
            else
            {
                await JoinQueue(now);
            }
            return true;
        }


        /// <summary>
        /// 检查延时
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private async Task<bool> CheckDelay(long delay)
        {
            var now = NowTime();
            if (Option.QueuePassBy)
            {
                await JoinQueue(RealInfo.PlanTime + delay);
                return true;
            }
            //空跳
            var time = RealInfo.PlanTime;
            int cnt = RealInfo.ExecNum + RealInfo.SkipNum;
            while (cnt == 0 || Option.PlanRepet < 0 || Option.PlanRepet > cnt)
            {
                time += delay;
                if (time >= now)
                {
                    await JoinQueue(time);
                    return true;
                }
                cnt++;
                ++RealInfo.SkipNum;
            }
            //结束仍在当前时间之前
            await Close();
            return true;
        }

        /// <summary>
        /// 检查几号
        /// </summary>
        /// <returns></returns>
        private Task<bool> CheckMonth()
        {
            return CheckMonth(FromTime(RealInfo.PlanTime));
        }

        /// <summary>
        /// 检查几号
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckMonth(DateTime baseDate)
        {
            int max = new DateTime(baseDate.Year, baseDate.Month, 1)
                .AddMonths(1)
                .AddDays(-1)
                .Day;

            DateTime next;

            if (Option.PlanValue > 0) //几号
            {
                var day = Option.PlanValue > max ? max : Option.PlanValue;

                next = new DateTime(baseDate.Year, baseDate.Month, day)
                           .AddMilliseconds(Option.PlanTime);
            }
            else
            {
                var day = max + Option.PlanValue < 0 ? 0 - max : Option.PlanValue;
                next = new DateTime(baseDate.Year, baseDate.Month, 1)
                            .AddMonths(1)
                            .AddDays(day)
                            .AddMilliseconds(Option.PlanTime);
            }
            if (!Option.QueuePassBy && next < DateTime.Now)
            {
                return await CheckMonth(next.AddMonths(1));
            }

            await JoinQueue((long)(next - baseTime).TotalMilliseconds);
            return true;
        }

        /// <summary>
        /// 检查周几
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckWeek()
        {
            var week = Option.PlanValue >= 7 ? 0 : Option.PlanValue;

            DateTime next = FromTime(RealInfo.PlanTime)
                .AddDays(week - (int)DateTime.Today.DayOfWeek)
                .AddMilliseconds(Option.PlanTime);

            while (!Option.QueuePassBy && next < DateTime.Now)
            {
                next = next.AddDays(7);
            }
            await JoinQueue((long)(next - baseTime).TotalMilliseconds);
            return true;
        }

        /// <summary>
        /// 加入执行队列
        /// </summary>
        /// <param name="time"></param>
        private async Task JoinQueue(long time)
        {
            if (Option.SkipSet > 0 && Option.SkipSet > RealInfo.SkipNum)
            {
                RealInfo.PlanState = PlanMessageState.skip;
            }
            else if (RealInfo.PlanState != PlanMessageState.retry)
            {
                RealInfo.PlanState = PlanMessageState.queue;
            }

            logger.Trace(() => $"Plan is queue.{Option.PlanId},state {RealInfo.PlanState},retry {RealInfo.RetryNum}");

            RealInfo.PlanTime = time;

#if !UNIT_TEST
            await RedisHelper.HSetAsync(Key, RealKey, SmartSerializer.ToInnerString(RealInfo));
            await RedisHelper.LRemAsync(planErrorKey, 0, Option.PlanId);
            await RedisHelper.ZAddAsync(planSetKey, (time, Option.PlanId));
            await RedisHelper.LRemAsync(planErrorKey, 0, Option.PlanId);
            await RedisHelper.LRemAsync(planPauseKey, 0, Option.PlanId);
            await RedisHelper.HDelAsync(planDoingKey, Option.PlanId);
#endif
        }

        #endregion

        #region 内容存取

        /// <summary>
        /// 读取计划任务节点
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loadMsg"></param>
        /// <returns></returns>
        public static async Task<PlanItem> LoadPlan(string id, bool loadMsg, bool loadMonitor)
        {
            var key = $"msg:plan:{id}";
            var op = await RedisHelper.HGetAsync(key, OptionKey);
            var ri = await RedisHelper.HGetAsync(key, RealKey);
            var item = new PlanItem
            {
                Option = SmartSerializer.FromInnerString<PlanOption>(op),
                RealInfo = SmartSerializer.FromInnerString<PlanRealInfo>(ri)
            };
            if (loadMsg)
            {
                var msg = await RedisHelper.HGetAsync(key, MessageKey);
                item.Message = SmartSerializer.FromInnerString<InlineMessage>(msg);
            }
            if (loadMonitor)
            {
                var monitor = await RedisHelper.HGetAsync(key, MonitorKey);
                if (monitor != "{}")
                    item.Monitor = SmartSerializer.FromInnerString<MonitorItem>(monitor);
            }
            if (item.Monitor == null)
            {
                item.Monitor = new MonitorItem();
                item.Monitor.BeginMonitor("计划执行");
            }

            return item;
        }
        #endregion
    }
}