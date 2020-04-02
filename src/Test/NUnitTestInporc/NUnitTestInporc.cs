using Agebull.Common.Ioc;
using NUnit.Framework;
using System.Threading.Tasks;
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
            IocHelper.AddTransient<IFlowMiddleware, ZmqFlowMiddleware>();//ZMQ环境,与ZeroRpcFlow冲突,只用其一
            IocHelper.AddTransient<IMessagePoster, InporcProducer>();//采用ZMQ进程内通讯生产端
            IocHelper.AddTransient<IMessageConsumer, InporcConsumer>();//采用ZMQ进程内通讯生产端
            IocHelper.AddTransient<ITransportDiscory, TestDiscory>();//网络协议发现
            IocHelper.ServiceCollection.UseTest(typeof(RpcControler).Assembly);
        }

        [Test]
        public async Task Test1()
        {
            for (int i = 0; i < 1000; i++)
            {
               await MessagePoster.PublishAsync("Markpoint2", "test", new Argument { Value = "Test" });
            }
            //Assert.IsTrue(task.Result.Success, "测试通过");
        }
    }
}