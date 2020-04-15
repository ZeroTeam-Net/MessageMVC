using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class JsonTest
    {
        const string json = "{'plan_type':'second'}";

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void Test()
        {
            var serialize = new NewtonJsonSerializeProxy();
            var plan = serialize.ToObject<PlanOption>(json);
            Assert.IsTrue(plan.plan_type == plan_date_type.second, json);
        }
    }
}



