using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    public class PlanItem
    {
        #region 系统配置与常量

        private int plan_auto_remove => PlanSystemOption.Option.CloseTimeout;

        private const string OptionKey = "opt";
        private const string RealKey = "rea";
        private const string MessageKey = "msg";

        private string Key => $"msg:plan:{Option.plan_id}";

        private const string planIdKey = "msg:plan:id";
        private const string planSetKey = "plan:time:set";
        private const string planDoingKey = "plan:time:do";
        private const string planErrorKey = "plan:time:err";
        private const string planPauseKey = "plan:time:pau";

        private const int second2ms = 1000;
        private const int minute2ms = 60 * 1000;
        private const int hour2ms = 60 * 60 * 1000;
        private const int day2ms = 24 * 60 * 60 * 1000;

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
        /// 实时信息
        /// </summary>
        public PlanRealInfo RealInfo { get; set; }

        /// <summary>
        /// 计划对象
        /// </summary>
        public IMessageItem Message { get; set; }

        #endregion

        #region 状态处理

        /// <summary>
        /// 首次保存
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FirstSave()
        {
            var now = NowTime();
#if !UNIT_TEST
            Option.plan_id = RedisHelper.IncrBy(planIdKey).ToString();
#else
            Option.plan_id = RandomCode.Generate(10);
#endif
            Option.add_time = now;
            if (Option.plan_time <= 0)
                Option.plan_time = now;
            RealInfo = new PlanRealInfo
            {
                plan_time = Option.plan_time
            };
            var tm = Option.plan_time;
            switch (Option.plan_type)
            {
                case plan_date_type.second:
                case plan_date_type.minute:
                case plan_date_type.hour:
                case plan_date_type.day:
                case plan_date_type.time:
                    //无效设置,自动放弃
                    if (Option.plan_value <= 0)
                        return false;
                    break;
                case plan_date_type.week:
                    //无效设置,自动放弃
                    if (Option.plan_value < 0 || Option.plan_value > 6)
                        return false;
                    Option.plan_time = (long)FromTime(tm).TimeOfDay.TotalMilliseconds;
                    RealInfo.plan_time = ToTime(tm > day2ms ? FromTime(Option.plan_time).Date : DateTime.Today);
                    break;
                case plan_date_type.month:
                    Option.plan_time = (long)FromTime(tm).TimeOfDay.TotalMilliseconds;
                    RealInfo.plan_time = ToTime(tm > day2ms ? FromTime(Option.plan_time).Date : DateTime.Today);
                    break;
                default:
                    return false;
            }

#if !UNIT_TEST
            await RedisHelper.HSetAsync(Key, OptionKey, Option);
            await RedisHelper.HSetAsync(Key, RealKey, RealInfo);
            await RedisHelper.HSetAsync(Key, MessageKey, Message);
#endif
            return true;
        }

        /// <summary>
        /// 设置为错误状态
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Error()
        {
            RealInfo.plan_state = Plan_message_state.error;

#if !UNIT_TEST
            await RemoveDoing();
            await RedisHelper.LPushAsync(planErrorKey, Option.plan_id);
            await RedisHelper.HSetAsync(Key, RealKey, RealInfo);
            await RedisHelper.ZRemAsync(planSetKey, Option.plan_id);
#endif
            return true;
        }

        private Task RemoveDoing()
        {
#if !UNIT_TEST
            return RedisHelper.HDelAsync(planDoingKey, Option.plan_id);
#else   
            return Task.CompletedTask;
#endif
        }

        /// <summary>
        /// 重试
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReTry()
        {
            RealInfo.plan_state = Plan_message_state.retry;
            if (++RealInfo.retry_num >= PlanSystemOption.Option.RetryCount)
            {
                await Close();
                return false;
            }

            var timeout = PlanSystemOption.Option.RetryDelay.Length < RealInfo.retry_num
                ? PlanSystemOption.Option.RetryDelay[RealInfo.retry_num]
                : PlanSystemOption.Option.RetryDelay[PlanSystemOption.Option.RetryDelay.Length - 1];

            await JoinQueue(NowTime() + timeout);

            return true;
        }

        /// <summary>
        /// 恢复执行
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Reset()
        {
            RealInfo.plan_state = Plan_message_state.none;
            await JoinQueue(RealInfo.plan_time);
            return true;
        }

        /// <summary>
        /// 暂停执行
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Pause()
        {
            RealInfo.plan_state = Plan_message_state.pause;

#if !UNIT_TEST
            await RemoveDoing();
            await RedisHelper.LPushAsync(planPauseKey, Option.plan_id);
            await RedisHelper.HSetAsync(Key, RealKey, RealInfo);
            await RedisHelper.ZRemAsync(planSetKey, Option.plan_id);
#endif
            //plan_dispatcher.instance->zero_event(zero_net_event.event_plan_pause, this);
            return true;
        }

        /// <summary>
        /// 设置跳过
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public async Task<bool> Skip(int set)
        {
            if (set < 0)
            {
                Option.skip_set = -1;
            }
            else
            {
                Option.skip_set = set;
            }
#if !UNIT_TEST
            await RedisHelper.HSetAsync(Key, OptionKey, Option);
#endif
            if (Option.skip_set >= RealInfo.skip_num)
            {
                await Close();
                return false;
            }
            return await CheckNextTime();
        }

        /// <summary>
        /// 关闭一个消息
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Close()
        {
#if !UNIT_TEST
            if (plan_auto_remove > 0)
#endif
            {
                RealInfo.plan_state = Plan_message_state.close;
#if !UNIT_TEST
                await RedisHelper.HSetAsync(Key, RealKey, RealInfo);
                await RedisHelper.ExpireAsync(Key, plan_auto_remove);

                await RedisHelper.ZRemAsync(planSetKey, Option.plan_id);

                await RedisHelper.LRemAsync(planErrorKey, 0, Option.plan_id);
                await RedisHelper.LRemAsync(planPauseKey, 0, Option.plan_id);
                await RemoveDoing();
            }
            else
            {
                await Remove(Option.plan_id);
#endif
            }
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
            //plan_dispatcher.instance->zero_event(zero_net_event.event_plan_remove, this);
            return true;
        }
        #endregion

        #region 执行当前

        /// <summary>
        /// 启动,
        /// </summary>
        public static async Task Start()
        {
            var ids = await RedisHelper.HGetAllAsync<long>(planDoingKey);
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    await RedisHelper.ZAddAsync(planSetKey, (id.Value, id.Key));
                }
            }
            await RedisHelper.DelAsync(planDoingKey);
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

            var message = LoadMessage(member, false);
            if (message == null || message.Option == null || message.RealInfo == null)
            {
                await Remove(member);
                return (true, null);
            }
            //设置为跳过的,直接计算下一次
            if (message.RealInfo.plan_state == Plan_message_state.skip)
            {
                ++message.RealInfo.skip_num;
                await message.CheckNextTime();
                return (true, null);
            }
            message.Message = RedisHelper.HGet<MessageItem>(message.Key, MessageKey);
            if (message.Message == null)
            {
                await Remove(member);
                return (true, null);
            }

            await RedisHelper.HSetAsync(planDoingKey, member, score);
            await RedisHelper.ZRemAsync(planSetKey, member);

            message.RealInfo.plan_state = Plan_message_state.execute;
            message.RealInfo.exec_num++;
            message.RealInfo.exec_start_time = NowTime();
            message.RealInfo.exec_state = MessageState.None;
            await RedisHelper.HSetAsync(message.Key, RealKey, message.RealInfo);

            return (true, message);
        }
        #endregion

        #region 执行时间计算

        /// <summary>
        /// 保存下一次执行时间
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckNextTime()
        {
            if (await DoCheckNext())
            {
                return true;
            }
            await Error();
            return false;
        }

        /// <summary>
        /// 计算下一次执行时间
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoCheckNext()
        {
            if ((Option.plan_repet > 0 && Option.plan_repet <= RealInfo.exec_num + RealInfo.skip_num) ||
                Option.plan_repet == 0 && RealInfo.exec_num > 0)
            {
                await Close();
                return true;
            }
            switch (Option.plan_type)
            {
                case plan_date_type.time:
                    return await CheckTime();
                case plan_date_type.second:
                    return await CheckDelay(Option.plan_value * second2ms);
                case plan_date_type.minute:
                    return await CheckDelay(Option.plan_value * minute2ms);
                case plan_date_type.hour:
                    return await CheckDelay(Option.plan_value * hour2ms);
                case plan_date_type.day:
                    return await CheckDelay(Option.plan_value * day2ms);
                case plan_date_type.week:
                    return await CheckWeek();
                case plan_date_type.month:
                    return await CheckMonth();
                default:
                    return false;
            }
        }


        /// <summary>
        /// 检查时间
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckTime()
        {
            var now = NowTime();
            if (RealInfo.success_num > 0)
            {
                await Close();
            }
            else if (RealInfo.plan_time <= 0)
            {
                await JoinQueue(now);
            }
            else if (Option.queue_pass_by)
            {
                await JoinQueue(RealInfo.plan_time);
            }
            else if (now >= RealInfo.plan_time)
            {
                RealInfo.skip_num = 1;
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
            if (Option.queue_pass_by)
            {
              await  JoinQueue(RealInfo.plan_time + delay);
                return true;
            }
            //空跳
            var time = RealInfo.plan_time;
            int cnt = RealInfo.exec_num + RealInfo.skip_num;
            while (cnt == 0 || Option.plan_repet < 0 || Option.plan_repet > cnt)
            {
                time += delay;
                if (time >= now)
                {
                    await JoinQueue(time);
                    return true;
                }
                cnt++;
                ++RealInfo.skip_num;
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
            return CheckMonth(FromTime(RealInfo.plan_time));
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

            if (Option.plan_value > 0) //几号
            {
                var day = Option.plan_value > max ? max : Option.plan_value;

                next = new DateTime(baseDate.Year, baseDate.Month, day)
                           .AddMilliseconds(Option.plan_time);
            }
            else
            {
                var day = max + Option.plan_value < 0 ? 0 - max : Option.plan_value;
                next = new DateTime(baseDate.Year, baseDate.Month, 1)
                            .AddMonths(1)
                            .AddDays(day)
                            .AddMilliseconds(Option.plan_time);
            }
            if (!Option.queue_pass_by && next < DateTime.Now)
                return await CheckMonth(next.AddMonths(1));

            await JoinQueue((long)(next - baseTime).TotalMilliseconds);
            return true;
        }

        /// <summary>
        /// 检查周几
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckWeek()
        {
            var week = Option.plan_value >= 7 ? 0 : Option.plan_value;

            DateTime next = FromTime(RealInfo.plan_time)
                .AddDays(week - (int)DateTime.Today.DayOfWeek)
                .AddMilliseconds(Option.plan_time);

            while (!Option.queue_pass_by && next < DateTime.Now)
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
            if (Option.skip_set > 0 && Option.skip_set > RealInfo.skip_num)
            {
                RealInfo.plan_state = Plan_message_state.skip;
            }
            else
            {
                RealInfo.plan_state = Plan_message_state.queue;
            }

            RealInfo.plan_time = time;

#if !UNIT_TEST
            await RedisHelper.HSetAsync(Key, RealKey, RealInfo);
            await RedisHelper.LRemAsync(planErrorKey, 0, Option.plan_id);
            await RedisHelper.ZAddAsync(planSetKey, (time, Option.plan_id));
            await RedisHelper.LRemAsync(planErrorKey, 0, Option.plan_id);
            await RedisHelper.LRemAsync(planPauseKey, 0, Option.plan_id);
            await RemoveDoing();
#endif
        }

        #endregion

        #region 内容存取

        /// <summary>
        /// 计划列表
        /// </summary>
        /// <returns></returns>
        public static List<PlanItem> LoadAll()
        {
            var messages = new List<PlanItem>();
            long cursor = 0;
            do
            {
                var redisScan = RedisHelper.Scan(cursor, "msg:plan:*");
                cursor = redisScan.Cursor;

                foreach (var key in redisScan.Items)
                {
                    var message = LoadMessage(key, true);
                    if (message == null)
                    {
                        RedisHelper.Del(key);
                        continue;
                    }
                    messages.Add(message);
                }
            } while (cursor > 0);
            return messages;
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loadMsg"></param>
        /// <returns></returns>
        private static PlanItem LoadMessage(string id, bool loadMsg)
        {
            var key = $"msg:plan:{id}";
            var msg = new PlanItem
            {
                Option = RedisHelper.HGet<PlanOption>(key, OptionKey),
                RealInfo = RedisHelper.HGet<PlanRealInfo>(key, RealKey)
            };
            if (loadMsg)
            {
                msg.Message = RedisHelper.HGet<MessageItem>(key, MessageKey);
            }

            return msg;
        }
        #endregion

        #region 返回值处理

        /// <summary>
        /// 保存消息参与者返回值
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<bool> SaveResult(string worker, string response)
        {
            if (!PlanSystemOption.Option.SavePlanResult)
            {
                return false;
            }
            await RedisHelper.HSetAsync(Key, worker, response);

            if (!await RedisHelper.HExistsAsync(Key, "works"))
            {
                await RedisHelper.HSetAsync(Key, "works", worker);
            }
            else
            {
                var works = await RedisHelper.HGetAsync(Key, "works");
                if (!works.Split(',').Contains(worker))
                {
                    await RedisHelper.HSetAsync(Key, "works", string.Join(',', works, worker));
                }
            }
            return true;
        }

        /// <summary>
        /// 取一个参与者的消息返回值
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public Task<string> LoadResult(string worker)
        {
            return RedisHelper.HGetAsync(Key, $"res:{worker}");
        }

        /// <summary>
        /// 取全部参与者消息返回值
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> LoadResult()
        {
            var result = new List<string>();
            if (!await RedisHelper.HExistsAsync(Key, "works"))
            {
                return result;
            }

            var works = RedisHelper.HGet(Key, "works").Split(',');
            foreach (var work in works)
            {
                result.Add(await RedisHelper.HGetAsync(Key, $"res:{work}"));
            }
            return result;
        }
        #endregion
    }

}