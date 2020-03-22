using Agebull.Common.Ioc;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    /// 通过ZMQ实现的进程内通讯
    /// </summary>
    public static class ZeroMQInporc
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="waitEnd"></param>
        public static Task UseZeroMQ(Assembly assembly, bool waitEnd)
        {
            Console.WriteLine("Weconme ZeroTeam KafkaMVC");

            IocHelper.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取
            IocHelper.AddTransient<IFlowMiddleware, AddInImporter>();//插件载入
            IocHelper.AddTransient<IFlowMiddleware, ZmqProxy>();//ZMQ环境
            IocHelper.AddTransient<IMessageConsumer, InporcConsumer>();//采用ZMQ进程内通讯客户端
            IocHelper.AddTransient<IMessageProducer, ZmqProducer>();//采用ZMQ进程内通讯生产端
            IocHelper.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            //IocHelper.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            IocHelper.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行

            //消息存储与异常消息重新消费
            //IocHelper.AddTransient<IMessageMiddleware, StorageMiddleware>();
            //IocHelper.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();
            //主流程
            ZeroFlowControl.CheckOption();
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Initialize();
            if (waitEnd)
               return ZeroFlowControl.RunAwaiteAsync();
            else
                return ZeroFlowControl.RunAsync();
        }
    }
}
