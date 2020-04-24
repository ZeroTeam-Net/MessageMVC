using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
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
        static int isInitialized = 0;

        /// <summary>
        /// 检查并注入配置
        /// </summary>
        /// <param name="services"></param>
        public static void CheckOption(this IServiceCollection services)
        {
            if (DependencyHelper.ServiceCollection != services)
            {
                DependencyHelper.SetServiceCollection(services);
            }

            //序列化工具
            services.AddTransient<ISerializeProxy, NewtonJsonSerializeProxy>();
            services.AddTransient<IJsonSerializeProxy, NewtonJsonSerializeProxy>();
            services.AddTransient<IXmlSerializeProxy, XmlSerializeProxy>();

            //配置\依赖对象初始化,系统配置获取
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();
            //消息选择器
            services.AddTransient<IFlowMiddleware, MessagePoster>();
            //API路由与执行
            services.AddTransient<IMessageMiddleware, ApiExecuter>();
            //消息接收服务自动发现
            services.AddTransient<IReceiverDiscover, ReceiverDiscover>();
            //插件载入
            if (ZeroAppOption.Instance.EnableAddIn)
            {
                services.AddSingleton<IFlowMiddleware>(AddInImporter.Instance);
            }

            services.TryAddSingleton<IInlineMessage, InlineMessage>();
            //IZeroContext构造
            services.TryAddScoped<IZeroContext, ZeroContext>();
            services.TryAddTransient<IUser, UserInfo>();
            //序列化器
            services.TryAddTransient<ISerializeProxy, NewtonJsonSerializeProxy>();
            services.TryAddTransient<IJsonSerializeProxy, NewtonJsonSerializeProxy>();
            services.TryAddTransient<IXmlSerializeProxy, XmlSerializeProxy>();
            ZeroFlowControl.CheckOption();
            DependencyHelper.Update();
        }

        /// <summary>
        /// 启动主流程控制器，发现指定类型所在的程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services"></param>
        /// <param name="type">需要发现服务的程序集的类型之一</param>
        /// <param name="waitEnd"></param>
        public static Task UseFlow(this IServiceCollection services, Type type, bool waitEnd = false)
        {
            return UseFlow(services, type.Assembly, waitEnd);
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        /// <param name="waitEnd"></param>
        public static async Task UseFlow(this IServiceCollection services, Assembly assembly, bool waitEnd = false)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                CheckOption(services);
                ZeroFlowControl.Discove(assembly);
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
            if (waitEnd)
            {
                await ZeroFlowControl.WaitEnd();
            }
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，适用于测试的场景
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static bool UseTest(this IServiceCollection services, Assembly assembly = null)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                services.TryAddTransient<IServiceReceiver, EmptyReceiver>();
                services.TryAddTransient<IMessageConsumer, EmptyReceiver>();
                services.TryAddTransient<INetEvent, EmptyReceiver>();
                CheckOption(services);
                if (assembly != null)
                    ZeroFlowControl.Discove(assembly);
            }
            ZeroFlowControl.Initialize();
            return ZeroFlowControl.Run();
        }

        /// <summary>
        /// 启动主流程控制器，不发现Api，适用于无Controler的场景
        /// </summary>
        /// <param name="services"></param>
        public static Task UseFlow(this IServiceCollection services)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                CheckOption(services);
            }
            ZeroFlowControl.Initialize();
            return ZeroFlowControl.RunAsync();
        }


        /// <summary>
        /// 启动主流程控制器，同时自动发现Api
        /// </summary>
        /// <param name="services"></param>
        public static async void UseFlowByAutoDiscover(this IServiceCollection services)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                CheckOption(services);
                ZeroFlowControl.Discove();
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

        /// <summary>
        /// 启动主流程控制器并等待退出，同时自动发现Api
        /// </summary>
        /// <param name="services"></param>
        public static async Task UseFlowAndWait(this IServiceCollection services)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                CheckOption(services);
                ZeroFlowControl.Discove();
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
            await ZeroFlowControl.WaitEnd();
        }
    }
}