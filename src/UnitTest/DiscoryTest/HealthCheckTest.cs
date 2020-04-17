using Agebull.Common.Ioc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class HealthCheckTest
    {
        [SetUp]
        public void Setup()
        {
            DependencyHelper.ServiceCollection.UseCsRedis();
            DependencyHelper.ServiceCollection.UseKafka();
            DependencyHelper.Update();
            DependencyHelper.ServiceCollection.UseTest(typeof(JsonTest).Assembly);
        }

        [TearDown]
        public void TearDown()
        {
            ZeroFlowControl.Shutdown();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Test()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "_HealthCheck_"
            });

            msg.OfflineResult(ser);

            Assert.IsTrue(msg.State != MessageState.Success, msg.Result);
        }
    }
}



