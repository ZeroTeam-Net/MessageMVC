using Agebull.Common.Ioc;
using NUnit.Framework;
using System;

namespace ZeroTeam.MessageMVC.UnitTest
{
    /// <summary>
    /// 插件测试
    /// </summary>
    [TestFixture]
    public class AddInTest
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ZeroTeam.MessageMVC.ZeroApp.UseTest(DependencyHelper.ServiceCollection).Wait();
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



