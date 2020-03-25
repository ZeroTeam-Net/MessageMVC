using Agebull.Common.Ioc;
using NUnit.Framework;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Sample.Controllers;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace NUnitTestInporc
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            IocHelper.AddTransient<IFlowMiddleware, ZmqProxy>();//ZMQ环境,与ZeroRpcFlow冲突,只用其一
            IocHelper.AddTransient<IMessageProducer, InporcProducer>();//采用ZMQ进程内通讯生产端
            IocHelper.AddTransient<IMessageConsumer, InporcConsumer>();//采用ZMQ进程内通讯生产端
            IocHelper.AddTransient<ITransportDiscory, TestDiscory>();//网络协议发现
            IocHelper.ServiceCollection.UseTest(typeof(RpcControler).Assembly);
        }

        [Test]
        public void Test1()
        {
            var task = MessageProducer.ProducerAsync<Argument, ApiResult>("Markpoint2", "test", new Argument { Value = "Test" });
            task.Wait();
            Assert.IsTrue(task.Result.Success, "测试通过");
        }
    }
}