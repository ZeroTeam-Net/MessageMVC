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
            IocHelper.AddTransient<IFlowMiddleware, ZmqFlowMiddleware>();//ZMQ����,��ZeroRpcFlow��ͻ,ֻ����һ
            IocHelper.AddTransient<IMessagePoster, InporcProducer>();//����ZMQ������ͨѶ������
            IocHelper.AddTransient<IMessageConsumer, InporcConsumer>();//����ZMQ������ͨѶ������
            IocHelper.AddTransient<ITransportDiscory, TestDiscory>();//����Э�鷢��
            IocHelper.ServiceCollection.UseTest(typeof(RpcControler).Assembly);
        }

        [Test]
        public async Task Test1()
        {
            for (int i = 0; i < 1000; i++)
            {
               await MessagePoster.PublishAsync("Markpoint2", "test", new Argument { Value = "Test" });
            }
            //Assert.IsTrue(task.Result.Success, "����ͨ��");
        }
    }
}