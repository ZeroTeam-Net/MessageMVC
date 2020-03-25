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
            IocHelper.AddTransient<IFlowMiddleware, ZmqProxy>();//ZMQ����,��ZeroRpcFlow��ͻ,ֻ����һ
            IocHelper.AddTransient<IMessageProducer, InporcProducer>();//����ZMQ������ͨѶ������
            IocHelper.AddTransient<IMessageConsumer, InporcConsumer>();//����ZMQ������ͨѶ������
            IocHelper.AddTransient<ITransportDiscory, TestDiscory>();//����Э�鷢��
            IocHelper.ServiceCollection.UseTest(typeof(RpcControler).Assembly);
        }

        [Test]
        public void Test1()
        {
            var task = MessageProducer.ProducerAsync<Argument, ApiResult>("Markpoint2", "test", new Argument { Value = "Test" });
            task.Wait();
            Assert.IsTrue(task.Result.Success, "����ͨ��");
        }
    }
}