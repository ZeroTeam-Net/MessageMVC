using System;
using System.Collections.Generic;
using System.Linq;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    public class PlanItem
    {
        #region ϵͳ�����볣��


        private int plan_exec_timeout => PlanSystemOption.Option.ExecTimeout;

        private int plan_auto_remove => PlanSystemOption.Option.CloseTimeout;

        private const string OptionKey = "opt";
        private const string RealKey = "rea";
        private const string MessageKey = "msg";

        private string Key => $"msg:plan:{Option.plan_id}";

        private const string planSetKey = "plan:time:set";
        private const string planDoingKey = "plan:time:do";
        private const string planErrorKey = "plan:time:err";
        private const string planPauseKey = "plan:time:pau";

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
        /// ���ݼ��
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            if (Option.plan_repet == 0 || (RealInfo.skip_set > 0 && Option.plan_repet > 0 && Option.plan_repet <= RealInfo.skip_set))
            {
                return false;
            }
            switch (Option.plan_type)
            {
                case plan_date_type.second:
                case plan_date_type.minute:
                case plan_date_type.hour:
                case plan_date_type.day:
                    //��Ч����,�Զ�����
                    if (Option.plan_value <= 0)
                    {
                        return false;
                    }
                    return true;
                case plan_date_type.week:
                    //��Ч����,�Զ�����
                    if (Option.plan_value < 0 || Option.plan_value > 6)
                    {
                        return false;
                    }
                    return true;
                case plan_date_type.time:
                case plan_date_type.month:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// ����Ϊ����״̬
        /// </summary>
        /// <returns></returns>
        public bool Error()
        {
            RedisHelper.LPush(planErrorKey, Option.plan_id);
            RemoveDoing();

            RealInfo.plan_state = Plan_message_state.error;
            RedisHelper.HSet(Key, RealKey, RealInfo);
            RedisHelper.ZRem(planSetKey, Option.plan_id);
            return true;
        }

        private void RemoveDoing() => RedisHelper.HDel(planDoingKey, Option.plan_id);
        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public bool ReTry()
        {
            RealInfo.plan_state = Plan_message_state.retry;
            if (++RealInfo.retry_repet >= PlanSystemOption.Option.RetryCount)
            {
                Close();
                return false;
            }

            var timeout = PlanSystemOption.Option.RetryDelay.Length < RealInfo.retry_repet
                ? PlanSystemOption.Option.RetryDelay[RealInfo.retry_repet]
                : PlanSystemOption.Option.RetryDelay[PlanSystemOption.Option.RetryDelay.Length - 1];

            join_queue(Time() + timeout);

            return true;
        }

        /// <summary>
        /// �ָ�ִ��
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            RealInfo.plan_state = Plan_message_state.none;
            join_queue(RealInfo.plan_time);
            return true;
        }

        /// <summary>
        /// ��ִͣ��
        /// </summary>
        /// <returns></returns>
        public bool Pause()
        {
            RemoveDoing();
            RedisHelper.LPush(planPauseKey, Option.plan_id);
            RealInfo.plan_state = Plan_message_state.pause;

            RedisHelper.HSet(Key, RealKey, RealInfo);
            RedisHelper.ZRem(planSetKey, Option.plan_id);
            //plan_dispatcher.instance->zero_event(zero_net_event.event_plan_pause, this);
            return true;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public bool Skip(int set)
        {
            if (set < 0)
            {
                RealInfo.skip_set = -1;
            }
            else
            {
                RealInfo.skip_set = set;
            }

            RedisHelper.HSet(Key, RealKey, RealInfo);
            if (RealInfo.skip_set >= RealInfo.skip_num)
            {
                Close();
                return false;
            }
            return true;
        }

        /// <summary>
        /// �ر�һ����Ϣ
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            RemoveDoing();

            RealInfo.plan_state = Plan_message_state.close;
            RedisHelper.HSet(Key, RealKey, RealInfo);
            RedisHelper.ZRem(planSetKey, Option.plan_id);

            if (plan_auto_remove > 0)
            {
                RedisHelper.Expire(Key, plan_auto_remove);
            }

            return true;
        }

        /// <summary>
        /// ɾ��һ����Ϣ
        /// </summary>
        /// <returns></returns>
        public static bool Remove(string id)
        {
            RedisHelper.LRem(planDoingKey, 0, id);
            RedisHelper.ZRem(planSetKey, id);
            RedisHelper.Del($"msg:plan:{id}");
            //plan_dispatcher.instance->zero_event(zero_net_event.event_plan_remove, this);
            return true;
        }
        #endregion

        #region ִ�е�ǰ

        /// <summary>
        /// ����,
        /// </summary>
        public static void Start()
        {
            var ids = RedisHelper.HGetAll<long>(planDoingKey);
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    RedisHelper.ZAdd(planSetKey, (id.Value, id.Key));
                }
            }
            RedisHelper.Del(planDoingKey);
        }

        /// <summary>
        /// �����ִ�еĵ�һ������
        /// </summary>
        /// <returns></returns>
        public static (bool state, PlanItem item) Pop()
        {
            var now = Time();
            var id = RedisHelper.ZRangeByScoreWithScores(planSetKey, 0, now, 1).FirstOrDefault();
            if (id.member == null)
            {
                return (false, null);
            }

            var message = load_message(id.member, false);
            if (message == null)
            {
                Remove(id.member);
                return (true, null);
            }
            //����Ϊ������,��ִֹ��
            if (message.RealInfo.skip_set == -2)
            {
                ++message.RealInfo.skip_num;
                message.Pause();
                return (true, null);
            }
            else if (message.RealInfo.skip_set == -1 ||
                (message.RealInfo.skip_set > 0 && message.RealInfo.skip_set > message.RealInfo.skip_num))
            {
                ++message.RealInfo.skip_num;
                message.CheckNextTime();
                return (true, null);
            }

            RedisHelper.HSet(planDoingKey, id.member, id.score);
            RedisHelper.ZRem(planSetKey, id);

            message.RealInfo.exec_repet++;
            message.RealInfo.exec_start_time = Time();
            message.RealInfo.exec_state = MessageState.None;
            RedisHelper.HSet(message.Key, RealKey, message.RealInfo);

            message.Message = RedisHelper.HGet<MessageItem>(message.Key, MessageKey);

            return (true, message);
        }
        #endregion

        #region ִ��ʱ�����

        /// <summary>
        /// ������һ��ִ��ʱ��
        /// </summary>
        /// <returns></returns>
        public bool CheckNextTime()
        {
            if (RealInfo == null)
            {
                Option.plan_id = RedisHelper.IncrBy("msg:plan:id").ToString();
                RealInfo = new PlanRealInfo
                {
                    add_time = Time(),
                    skip_set = Option.skip_set,
                    plan_time = Option.plan_time
                };
                RedisHelper.HSet(Key, OptionKey, Option);
                RedisHelper.HSet(Key, RealKey, RealInfo);
                RedisHelper.HSet(Key, MessageKey, Message);
            }
            else if (RealInfo.plan_state == Plan_message_state.pause)
            {
                RealInfo.skip_set = Option.skip_set;
                RealInfo.skip_num = 0;
            }

            if (DoCheckNext())
            {
                return true;
            }

            Error();
            return false;
        }

        /// <summary>
        /// ������һ��ִ��ʱ��
        /// </summary>
        /// <returns></returns>
        private bool DoCheckNext()
        {
            if (Option.plan_repet > 0 && Option.plan_repet <= RealInfo.real_repet)
            {
                Close();
                return true;
            }
            switch (Option.plan_type)
            {
                case plan_date_type.time:
                    return check_time();
                case plan_date_type.second:
                    return check_delay(Option.plan_value * 1000);
                case plan_date_type.minute:
                    return check_delay(Option.plan_value * 60000);
                case plan_date_type.hour:
                    return check_delay(Option.plan_value * 3600000);
                case plan_date_type.day:
                    return check_delay(Option.plan_value * 3600000 * 24);
                case plan_date_type.week:
                    return check_week();
                case plan_date_type.month:
                    return check_month();
                default:
                    return false;
            }
        }

        private static readonly DateTime baseTime = new DateTime(1970, 1, 1);
        internal static long Time()
        {
            return (long)(DateTime.Now - baseTime).TotalMilliseconds;
        }

        private static DateTime FromTime(long ms)
        {
            return baseTime.AddMilliseconds(ms);
        }


        /// <summary>
        /// ���ʱ��
        /// </summary>
        /// <returns></returns>
        private bool check_time()
        {
            if (RealInfo.real_repet > 0)
            {
                Close();
            }
            else if (RealInfo.plan_time <= 0)
            {
                join_queue(Time());
            }
            else if (Option.no_skip)
            {
                join_queue(RealInfo.plan_time);
            }
            else if (Time() > RealInfo.plan_time)
            {
                RealInfo.skip_num = 1;
                Close();
            }
            else
            {
                join_queue(Time());
            }
            return true;
        }

        /// <summary>
        /// ��鼸��
        /// </summary>
        /// <returns></returns>
        private bool check_month()
        {
            int day;
            int max = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1).Day;
            if (Option.plan_value > 0) //����
            {
                day = Option.plan_value > max ? max : Option.plan_value;
            }
            else
            {
                int vl = 0 - Option.plan_value;
                day = vl <= max ? 1 : max - vl;
            }
            var time = FromTime(Option.plan_time).TimeOfDay.TotalMilliseconds;
            DateTime next = new DateTime(DateTime.Today.Year, DateTime.Today.Month, day).AddMilliseconds(time);
            if (next > DateTime.Now)
            {
                next = next.AddMonths(1);
            }
            join_queue(next);
            return true;
        }

        /// <summary>
        /// ����ܼ�
        /// </summary>
        /// <returns></returns>
        private bool check_week()
        {
            var time = FromTime(Option.plan_time).TimeOfDay.TotalMilliseconds;

            DateTime date = DateTime.Now.AddMilliseconds(time);


            int wk = (int)DateTime.Now.DayOfWeek;
            if (wk == Option.plan_value) //����
            {
                if (date > DateTime.Now)
                {
                    date = date.AddDays(7);
                }
            }
            else if (wk < Option.plan_value) //��û��
            {
                date.AddDays(Option.plan_value - wk);
            }
            else //����
            {
                date.AddDays(Option.plan_value - wk + 7);
            }
            join_queue(date);
            return true;
        }

        /// <summary>
        /// �����ʱ
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private bool check_delay(long delay)
        {
            var now = Time();
            var baseTime = RealInfo.plan_time <= 0 ? now : RealInfo.plan_time;
            if (Option.no_skip)
            {
                join_queue(baseTime + delay);
                return true;
            }
            //����
            int cnt = RealInfo.real_repet;
            while (Option.plan_repet < 0 || Option.plan_repet > cnt)
            {
                baseTime += delay;
                if (baseTime >= now)
                {
                    join_queue(baseTime);
                    return true;
                }
                cnt++;
                ++RealInfo.skip_num;
            }
            //�������ڵ�ǰʱ��֮ǰ
            Close();
            return true;
        }

        /// <summary>
        /// ����ִ�ж���
        /// </summary>
        /// <param name="dateTime"></param>
        private void join_queue(DateTime dateTime)
        {
            join_queue((long)(dateTime - baseTime).TotalSeconds);
        }

        /// <summary>
        /// ����ִ�ж���
        /// </summary>
        /// <param name="time"></param>
        private void join_queue(long time)
        {
            if (RealInfo.skip_set == 0 || (RealInfo.skip_set > 0 && RealInfo.skip_set < RealInfo.skip_num))
            {
                RealInfo.plan_state = Plan_message_state.queue;
            }
            else
            {
                RealInfo.plan_state = Plan_message_state.skip;
            }

            RealInfo.plan_time = (int)time;
            RedisHelper.HSet(Key, RealKey, RealInfo);
            RedisHelper.ZAdd(planSetKey, (time, Option.plan_id));

            RedisHelper.LRem(planErrorKey, 0, Option.plan_id);
            RedisHelper.LRem(planPauseKey, 0, Option.plan_id);
            RemoveDoing();
        }

        #endregion

        #region ���ݴ�ȡ

        /// <summary>
        /// �ƻ��б�
        /// </summary>
        /// <returns></returns>
        public static List<PlanItem> plan_list()
        {
            var messages = new List<PlanItem>();
            long cursor = 0;
            do
            {
                var redisScan = RedisHelper.Scan(cursor, "msg:plan:*");
                cursor = redisScan.Cursor;

                foreach (var key in redisScan.Items)
                {
                    var message = load_message(key, true);
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
        private static PlanItem load_message(string id, bool loadMsg)
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
        /// ������Ϣ������
        /// </summary>
        /// <param name="workers"></param>
        /// <returns></returns>
        public bool save_message_worker(List<string> workers)
        {
            if (!RedisHelper.HExists(Key, "works"))
            {
                RedisHelper.HSet(Key, "works", string.Join(',', workers));
            }
            else
            {
                workers.AddRange(RedisHelper.HGet(Key, "works").Split(','));
                RedisHelper.HSet(Key, "works", string.Join(',', workers.Distinct()));
            }
            return true;
        }

        /// <summary>
        /// ������Ϣ�����߷���ֵ
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public bool save_message_result(string worker, string response)
        {
            if (!PlanSystemOption.Option.SavePlanResult)
            {
                return false;
            }
            RedisHelper.HSet(Key, worker, response);

            if (!RedisHelper.HExists(Key, "works"))
            {
                RedisHelper.HSet(Key, "works", worker);
            }
            else
            {
                var works = RedisHelper.HGet(Key, "works");
                if (!works.Split(',').Contains(worker))
                {
                    RedisHelper.HSet(Key, "works", string.Join(',', works, worker));
                }
            }
            return true;
        }

        /// <summary>
        /// ȡһ�������ߵ���Ϣ����ֵ
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public string get_message_result(string worker) => RedisHelper.HGet($"plan:res:{Option.plan_id}", $"res:{worker}");

        /// <summary>
        /// ȡȫ����������Ϣ����ֵ
        /// </summary>
        /// <returns></returns>
        public List<string> get_message_result()
        {
            var result = new List<string>();
            if (!RedisHelper.HExists(Key, "works"))
            {
                return result;
            }

            var works = RedisHelper.HGet(Key, "works").Split(',');
            foreach (var work in works)
            {
                result.Add(RedisHelper.HGet(Key, $"res:{work}"));
            }
            return result;
        }
        #endregion
    }

}