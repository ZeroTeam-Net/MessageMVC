using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
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
        /// 检查并注入配置
        /// </summary>
        /// <param name="services"></param>
        private static void CheckOption(IServiceCollection services)
        {

            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取

            if (IocHelper.ServiceCollection != services)
            {
                IocHelper.SetServiceCollection(services);
            }

            ZeroFlowControl.CheckOption();

            services.AddTransient<IFlowMiddleware, MessagePoster>();//消息选择器
            if (ZeroFlowControl.Config.EnableAddIn)
            {
                services.AddTransient<IFlowMiddleware, AddInImporter>();//插件载入
            }

            if (ZeroFlowControl.Config.EnableLogRecorder)
            {
                services.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            }

            if (ZeroFlowControl.Config.EnableGlobalContext)
            {
                var testContext = IocHelper.Create<GlobalContext>();
                if (testContext == null)
                {
                    IocHelper.AddScoped<GlobalContext, GlobalContext>();
                }

                services.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            }
            if (ZeroFlowControl.Config.EnableMarkPoint)
            {
                services.AddSingleton<IMessageMiddleware, MarkPointMiddleware>();
            }
            //消息存储与异常消息重新消费
            if (ZeroFlowControl.Config.EnableMessageReConsumer)
            {
                services.AddTransient<IMessageMiddleware, StorageMiddleware>();
                services.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();
            }
            services.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行
            IocHelper.Update();
        }

        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        /// <param name="waitEnd"></param>
        public static Task UseFlow(this IServiceCollection services, Assembly assembly, bool waitEnd)
        {
            CheckOption(services);
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Initialize();
            if (waitEnd)
            {
                return ZeroFlowControl.RunAwaiteAsync();
            }
            else
            {
                return ZeroFlowControl.RunAsync();
            }
        }


        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static bool UseTest(this IServiceCollection services, Assembly assembly)
        {
            services.AddTransient<IFlowMiddleware, MessagePoster>();//消息选择器
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取
            services.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            //services.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            services.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行

            if (IocHelper.ServiceCollection != services)
            {
                IocHelper.SetServiceCollection(services);
            }

            IocHelper.Update();
            ZeroFlowControl.CheckOption();
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Initialize();
            return ZeroFlowControl.Run();
        }

        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        public static Task UseFlow(this IServiceCollection services)
        {
            CheckOption(services);
            ZeroFlowControl.Initialize();
            return ZeroFlowControl.RunAsync();
        }


        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        public static Task UseFlowByAutoDiscory(this IServiceCollection services)
        {
            CheckOption(services);
            ZeroFlowControl.Discove();
            ZeroFlowControl.Initialize();
            return ZeroFlowControl.RunAsync();
        }
    }
}