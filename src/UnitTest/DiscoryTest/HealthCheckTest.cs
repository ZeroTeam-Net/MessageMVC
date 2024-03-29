﻿using Agebull.Common.Ioc;
using NUnit.Framework;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class HealthCheckTest
    {
        [SetUp]
        public void Setup()
        {
            DependencyHelper.ServiceCollection.AddMessageMvcRedis();
            DependencyHelper.ServiceCollection.AddMessageMvcKafka();
            DependencyHelper.Flush();
            DependencyHelper.ServiceCollection.UseTest(typeof(ApiContraceJsonTest).Assembly);
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
            var msg = await MessagePoster.Post(new InlineMessage
            {
                Service = "UnitService",
                Method = "_health_"
            });

            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }
    }
}



