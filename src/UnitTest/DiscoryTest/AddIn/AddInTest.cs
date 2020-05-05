using Agebull.Common.Ioc;
using NUnit.Framework;
using System;

namespace ZeroTeam.MessageMVC.UnitTest
{
    [TestFixture]
    public class AddInTest
    {
        [SetUp]
        public void SetUp()
        {
            ZeroTeam.MessageMVC.ZeroApp.UseTest(DependencyHelper.ServiceCollection);
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void DoTest()
        {
            Console.WriteLine("DoTest");
        }
    }
}



