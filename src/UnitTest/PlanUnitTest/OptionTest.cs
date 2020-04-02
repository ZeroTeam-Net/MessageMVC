using NUnit.Framework;
using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;

namespace PlanUnitTest
{
    public class OptionTest
    {
        private const int second2ms = 1000;
        private const int minute2ms = 60 * 1000;
        private const int hour2ms = 60 * 60 * 1000;
        private const int day2ms = 24 * 60 * 60 * 1000;

        [SetUp]
        public void Setup()
        {
            //如果未设置 UNIT_TEST ,需要打开Redis
            //var config = ConfigurationManager.Get("Redis").GetStr("ConnectionString");
            //Trace.TraceInformation(config);
            //RedisHelper.Initialization(new CSRedisClient(config));

            //PlanSystemOption.Option = ConfigurationManager.Get<PlanSystemOption>("PlanTask")
            //                                ?? new PlanSystemOption();
        }

        [Test]
        public void TestSecond()
        {
            var now = PlanItem.NowTime() + minute2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.second,
                    plan_value = 10,
                    plan_time = now
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            item.CheckNextTime();
            now += 10 * second2ms;
            var next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{now}=>{item.RealInfo.plan_time} {next} : {item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            item.RealInfo.exec_num = 1;
            item.CheckNextTime();
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");

        }

        [Test]
        public void Testminute()
        {
            var now = PlanItem.NowTime() + minute2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.minute,
                    plan_value = 1,
                    plan_time = now
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            item.CheckNextTime();
            now += minute2ms;
            var next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{now}=>{item.RealInfo.plan_time} {next} : {item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            item.RealInfo.exec_num = 1;
            item.CheckNextTime();
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");

        }


        [Test]
        public void Testhour()
        {
            var now = PlanItem.NowTime() + minute2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.hour,
                    plan_value = 1,
                    plan_time = now
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            item.CheckNextTime();
            now += hour2ms;
            var next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{now}=>{item.RealInfo.plan_time} {next} : {item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            item.RealInfo.exec_num = 1;
            item.CheckNextTime();
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");

        }


        [Test]
        public void Testday()
        {
            var now = PlanItem.NowTime() + minute2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.day,
                    plan_value = 1,
                    plan_time = now
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            item.CheckNextTime();
            now += day2ms;
            var next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{now}=>{item.RealInfo.plan_time} {next} : {item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            item.RealInfo.exec_num = 1;
            item.CheckNextTime();
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");

        }


        [Test]
        public void Testweek()
        {
            var week = new Random(GetHashCode()).Next(0, 7);
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.week,
                    plan_value = (short)week,
                    plan_time = 1000
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = 0
                }
            };
            item.FirstSave();
            item.CheckNextTime();
            var next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{(DayOfWeek)week}=>{next.DayOfWeek} {next} : {item.RealInfo.plan_state}");
            Assert.IsTrue((int)next.DayOfWeek == week, $"结果时间不同{week}=>{(int)next.DayOfWeek}");

            item.RealInfo.exec_num = 1;
            item.CheckNextTime();
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");

        }

        [Test]
        public void TestMonth()
        {
            var day = (short)new Random(GetHashCode()).Next(0, 32);
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.month,
                    plan_value = day,
                    plan_time = 10000
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = PlanItem.NowTime()
                }
            };
            item.CheckNextTime();
            var next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{day}=>{(int)next.DayOfWeek} {next} : {item.RealInfo.plan_state}");
            Assert.IsTrue(next.Day == day, $"结果时间不同{day}=>{next.Day}");

            item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.month,
                    plan_value = -31,
                    plan_time = 10000
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = PlanItem.NowTime()
                }
            };
            item.CheckNextTime();
            next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{next} : {item.RealInfo.plan_state}");
            Assert.IsTrue(next.Day == 1, $"结果时间不同{next.Day}=>1");

            item.RealInfo.exec_num = 1;
            item.CheckNextTime();
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");

        }


        [Test]
        public void TestRepet()
        {
            var now = PlanItem.NowTime() + second2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.second,
                    plan_value = 1,
                    plan_repet = 30,
                    plan_time = now
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            for (int i = 0; i < 30; i++)
            {
                item.CheckNextTime();
                var next = PlanItem.FromTime(item.RealInfo.plan_time);
                item.RealInfo.exec_num++;//模拟成功
                now += second2ms;
                Console.WriteLine($"{PlanItem.FromTime(now)} => {next} : {item.RealInfo.plan_state}");
                Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.queue, $"状态错误{Plan_message_state.queue}=>{item.RealInfo.plan_state}");
                Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");
            }
            item.CheckNextTime();
            Console.WriteLine($"{PlanItem.FromTime(now)} => {PlanItem.FromTime(item.RealInfo.plan_time)} : {item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");
        }



        [Test]
        public void TestNumberSkip()
        {
            var now = PlanItem.NowTime() + second2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.second,
                    plan_value = 1,
                    plan_repet = 30,
                    plan_time = now,
                    skip_set = 29,
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            DateTime next;
            for (int i = 0; i < 29; i++)
            {
                item.CheckNextTime();
                item.RealInfo.skip_num++;//模拟成功

                now += second2ms;
                next = PlanItem.FromTime(item.RealInfo.plan_time);
                Console.WriteLine($"{PlanItem.FromTime(now)} => {next} : {item.RealInfo.plan_state} skip:{item.RealInfo.skip_num}");
                Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.skip, $"状态错误{Plan_message_state.queue}=>{item.RealInfo.plan_state}");
                Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            }

            item.CheckNextTime();
            item.RealInfo.exec_num++;//模拟成功

            now += second2ms;
            next = PlanItem.FromTime(item.RealInfo.plan_time);
            Console.WriteLine($"{PlanItem.FromTime(now)} => {next} : {item.RealInfo.plan_state} skip:{item.RealInfo.skip_num} exec:{item.RealInfo.exec_num}");
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.queue, $"状态错误{Plan_message_state.queue}=>{item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");


            item.CheckNextTime();

            Console.WriteLine($"{PlanItem.FromTime(now)} => {PlanItem.FromTime(item.RealInfo.plan_time)} : {item.RealInfo.plan_state}");
            Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.close, $"状态错误{Plan_message_state.close}=>{item.RealInfo.plan_state}");
        }


        [Test]
        public void TestAlwaysSkip()
        {
            var now = PlanItem.NowTime() + second2ms;
            var item = new PlanItem
            {
                Message = new MessageItem(),
                Option = new PlanOption
                {
                    plan_type = plan_date_type.second,
                    plan_value = 1,
                    plan_repet = -1,
                    plan_time = now,
                    skip_set = 29,
                },
                RealInfo = new PlanRealInfo
                {
                    plan_time = now
                }
            };
            DateTime next;
            for (int i = 0; i < 29; i++)
            {
                item.CheckNextTime();
                item.RealInfo.skip_num++;//模拟成功

                now += second2ms;
                next = PlanItem.FromTime(item.RealInfo.plan_time);
                Console.WriteLine($"{PlanItem.FromTime(now)} => {next} : {item.RealInfo.plan_state} skip:{item.RealInfo.skip_num} exec:{item.RealInfo.exec_num}");
                Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.skip, $"状态错误{Plan_message_state.queue}=>{item.RealInfo.plan_state}");
                Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            }

            for (int i = 0; i < 29; i++)
            {
                item.CheckNextTime();
                item.RealInfo.exec_num++;//模拟成功

                now += second2ms;
                next = PlanItem.FromTime(item.RealInfo.plan_time);
                Console.WriteLine($"{PlanItem.FromTime(now)} => {next} : {item.RealInfo.plan_state} skip:{item.RealInfo.skip_num} exec:{item.RealInfo.exec_num}");
                Assert.IsTrue(item.RealInfo.plan_state == Plan_message_state.queue, $"状态错误{Plan_message_state.queue}=>{item.RealInfo.plan_state}");
                Assert.IsTrue(item.RealInfo.plan_time == now, $"结果时间不同{now}=>{item.RealInfo.plan_time}");

            }
        }
    }
}