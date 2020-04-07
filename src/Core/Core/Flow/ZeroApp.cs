using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.AddIn;
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
        public static void CheckOption(this IServiceCollection services)
        {
            if (IocHelper.ServiceCollection != services)
            {
                IocHelper.SetServiceCollection(services);
            }
            //配置\依赖对象初始化,系统配置获取
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();
            //消息选择器
            services.AddTransient<IFlowMiddleware, MessagePoster>();
            //API路由与执行
            services.AddTransient<IMessageMiddleware, ApiExecuter>();
            //IZeroContext构造
            services.TryAddTransient<IZeroContext, ZeroContext>();
            //ApiResult构造
            services.TryAddTransient<IApiResultDefault, ApiResultDefault>();
            //消息接收服务自动发现
            services.AddTransient<IReceiverDiscory, ReceiverDiscory>();
            //插件载入
            if (ZeroAppOption.Instance.EnableAddIn)
            {
                services.AddSingleton<IFlowMiddleware>(AddInImporter.Instance);
            }

            ZeroFlowControl.CheckOption();
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
        public static async void UseFlowByAutoDiscory(this IServiceCollection services)
        {
            CheckOption(services);
            ZeroFlowControl.Discove();
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

        /// <summary>
        /// 使用主流程控制器
        /// </summary>
        /// <param name="services"></param>
        public static Task UseFlowAsync(this IServiceCollection services)
        {
            CheckOption(services);
            ZeroFlowControl.Discove();
            ZeroFlowControl.Initialize();
            return ZeroFlowControl.RunAwaiteAsync();
        }
    }
}