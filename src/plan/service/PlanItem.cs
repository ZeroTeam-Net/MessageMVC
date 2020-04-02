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
        #region ϵͳ�����볣��

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
        /// ��׼ʱ�� 2020-3-12
        /// </summary>
        public static readonly DateTime baseTime = new DateTime(2020, 3, 12);

        /// <summary>
        /// ��ǰʱ���
        /// </summary>
        /// <returns></returns>
        public static long NowTime() => (long)(DateTime.Now - baseTime).TotalMilliseconds;

        /// <summary>
        /// ʱ���תΪʱ��
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static DateTime FromTime(long ms) => baseTime.AddMilliseconds(ms);

        /// <summary>
        /// ʱ��תΪʱ���
        /// </summary>
        /// <param name="time">����С�ڻ�׼ʱ�� 2020-3-12</param>
        /// <returns></returns>
        public static long ToTime(DateTime time) => (long)(time - baseTime).TotalMilliseconds;

        #endregion

        #region �ƻ�����

        /// <summary>
        /// �ƻ�����
        /// </summary>
        public PlanOption Option { get; set; }

        /// <summary>
        /// ʵʱ��Ϣ
        /// </summary>
        public PlanRealInfo RealInfo { get; set; }

        /// <summary>
        /// �ƻ�����
        /// </summary>
        public IMessageItem Message { get; set; }

        #endregion

        #region ״̬����

        /// <summary>
        /// �״α���
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
                    //��Ч����,�Զ�����
                    if (Option.plan_value <= 0)
                        return false;
                    break;
                case plan_date_type.week:
                    //��Ч����,�Զ�����
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
        /// ����Ϊ����״̬
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
        /// ����
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
        /// �ָ�ִ��
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Reset()
        {
            RealInfo.plan_state = Plan_message_state.none;
            await JoinQueue(RealInfo.plan_time);
            return true;
        }

        /// <summary>
        /// ��ִͣ��
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
        /// ��������
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
        /// �ر�һ����Ϣ
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
        /// ɾ��һ����Ϣ
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

        #region ִ�е�ǰ

        /// <summary>
        /// ����,
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
        /// �����ִ�еĵ�һ������
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
            //����Ϊ������,ֱ�Ӽ�����һ��
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

        #region ִ��ʱ�����

        /// <summary>
        /// ������һ��ִ��ʱ��
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
        /// ������һ��ִ��ʱ��
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
        /// ���ʱ��
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
        /// �����ʱ
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
            //����
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
            //�������ڵ�ǰʱ��֮ǰ
            await Close();
            return true;
        }

        /// <summary>
        /// ��鼸��
        /// </summary>
        /// <returns></returns>
        private Task<bool> CheckMonth()
        {
            return CheckMonth(FromTime(RealInfo.plan_time));
        }

        /// <summary>
        /// ��鼸��
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckMonth(DateTime baseDate)
        {
            int max = new DateTime(baseDate.Year, baseDate.Month, 1)
                .AddMonths(1)
                .AddDays(-1)
                .Day;

            DateTime next;

            if (Option.plan_value > 0) //����
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
        /// ����ܼ�
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
        /// ����ִ�ж���
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

        #region ���ݴ�ȡ

        /// <summary>
        /// �ƻ��б�
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
        /// ��ȡ��Ϣ
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

        #region ����ֵ����

        /// <summary>
        /// ������Ϣ�����߷���ֵ
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
        /// ȡһ�������ߵ���Ϣ����ֵ
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public Task<string> LoadResult(string worker)
        {
            return RedisHelper.HGetAsync(Key, $"res:{worker}");
        }

        /// <summary>
        /// ȡȫ����������Ϣ����ֵ
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