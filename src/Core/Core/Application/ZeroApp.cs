using System.Reflection;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 应用扩展方法
    /// </summary>
    public static class ZeroApp
    {

        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        /// <param name="waitEnd"></param>
        public static Task UseFlow(this IServiceCollection services, Assembly assembly, bool waitEnd)
        {
            services.AddTransient<IFlowMiddleware, MessageProducer>();//消息选择器
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取
            services.AddTransient<IFlowMiddleware, AddInImporter>();//插件载入
            services.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            //services.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            services.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行
            //消息存储与异常消息重新消费
            //services.AddTransient<IMessageMiddleware, StorageMiddleware>();
            //services.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();

            if (IocHelper.ServiceCollection != services)
                IocHelper.SetServiceCollection(services);
            IocHelper.Update();
            ZeroFlowControl.CheckOption();
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Initialize();
            if (waitEnd)
                return ZeroFlowControl.RunAwaiteAsync();
            else
                return ZeroFlowControl.RunAsync();
        }


        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        public static void UseFlow(this IServiceCollection services)
        {
            services.AddTransient<IFlowMiddleware, MessageProducer>();//消息选择器
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取
            services.AddTransient<IFlowMiddleware, AddInImporter>();//插件载入
            services.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            //services.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            services.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行
            //消息存储与异常消息重新消费
            //services.AddTransient<IMessageMiddleware, StorageMiddleware>();
            //services.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();

            if (IocHelper.ServiceCollection != services)
                IocHelper.SetServiceCollection(services);
            IocHelper.Update();
            ZeroFlowControl.CheckOption();
            ZeroFlowControl.Initialize();

            Task.Factory.StartNew(ZeroFlowControl.Run,TaskCreationOptions.DenyChildAttach);
        }
    }
}