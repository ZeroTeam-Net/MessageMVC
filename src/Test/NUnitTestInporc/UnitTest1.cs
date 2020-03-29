using Agebull.Common.Ioc;
using NUnit.Framework;
<<<<<<< HEAD
using System.Threading.Tasks;
=======
>>>>>>> c9fa0596fe7d47bbd0bf81699d533fa3d886c8e2
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
<<<<<<< HEAD
        public async Task Test1()
        {
            for (int i = 0; i < 1000; i++)
            {
                MessageProducer.ProducerAsync<Argument, ApiResult>("Markpoint2", "test", new Argument { Value = "Test" });
            }
=======
        public void Test1()
        {
            var task = MessageProducer.ProducerAsync<Argument, ApiResult>("Markpoint2", "test", new Argument { Value = "Test" });
            task.Wait();
            Assert.IsTrue(task.Result.Success, "����ͨ��");
>>>>>>> c9fa0596fe7d47bbd0bf81699d533fa3d886c8e2
        }
    }
}