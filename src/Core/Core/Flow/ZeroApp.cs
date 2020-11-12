using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Diagnostics;
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
        static void AddDependency(IServiceCollection services, bool msJson)
        {
            //IZeroContext构造
            services.TryAddScoped<IZeroContext, ZeroContext>();
            //序列化工具
            services.TryAddTransient<ISerializeProxy, NewtonJsonSerializeProxy>();
            services.TryAddTransient<IJsonSerializeProxy, NewtonJsonSerializeProxy>();
            services.TryAddTransient<IXmlSerializeProxy, XmlSerializeProxy>();

            //配置\依赖对象初始化,系统配置获取
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();
            //消息选择器
            services.AddTransient<IFlowMiddleware, MessagePoster>();
            //API路由与执行
            services.AddTransient<IMessageMiddleware, ApiExecuter>();
            //并行发送器
            services.AddTransient<IFlowMiddleware, ParallelPoster>(); 
            //插件载入
            //if (ZeroAppOption.Instance.EnableAddIn)
            {
                services.AddSingleton<IFlowMiddleware>(pri => AddInImporter.Instance);
            }
            services.TryAddSingleton<IInlineMessage, InlineMessage>();

            if (msJson)
            {
                services.TryAddTransient<ISerializeProxy, JsonSerializeProxy>();
                services.TryAddTransient<IJsonSerializeProxy, JsonSerializeProxy>();
                services.TryAddTransient<IXmlSerializeProxy, XmlSerializeProxy>();
            }
            else
            {
                services.TryAddTransient<ISerializeProxy, NewtonJsonSerializeProxy>();
                services.TryAddTransient<IJsonSerializeProxy, NewtonJsonSerializeProxy>();
                services.TryAddTransient<IXmlSerializeProxy, XmlSerializeProxy>();
            }
        }

        /// <summary>
        /// 启动主流程控制器
        /// </summary>
        /// <param name="services">依赖服务</param>
        /// <param name="msJson">true 使用System.Text.Json,false 使用NewtonsoftJson</param>
        /// <param name="autoDiscover">对接口自动发现</param>
        public static void AddMessageMvc(this IServiceCollection services, bool autoDiscover = true, bool msJson = false)
        {
            if (Interlocked.Increment(ref isInitialized) != 1)
            {
                return;
            }
            AddDependency(services, msJson);
            ZeroFlowControl.Check();
            if (autoDiscover)
                ZeroFlowControl.Discove();
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services">依赖服务</param>
        /// <param name="msJson">true 使用System.Text.Json,false 使用NewtonsoftJson</param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static void AddMessageMvc(this IServiceCollection services, Assembly assembly, bool msJson = false)
        {
            if (Interlocked.Increment(ref isInitialized) != 1)
            {
                return;
            }
            AddDependency(services, msJson);
            ZeroFlowControl.Check();
            ZeroFlowControl.Discove(assembly);
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，适用于测试的场景
        /// </summary>
        /// <param name="services">依赖服务</param>
        /// <param name="msJson">true 使用System.Text.Json,false 使用NewtonsoftJson</param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static async Task UseTest(this IServiceCollection services, Assembly assembly = null, bool msJson = false)
        {
            if (Interlocked.Increment(ref isInitialized) != 1)
            {
                return;
            }
            services.TryAddTransient<IServiceReceiver, EmptyReceiver>();
            services.TryAddTransient<IMessageConsumer, EmptyReceiver>();
            services.TryAddTransient<INetEvent, EmptyReceiver>();
            AddDependency(services, msJson);
            ZeroFlowControl.Check();
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Check();
            if (assembly != null)
                ZeroFlowControl.Discove(assembly);
            await ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        public static async Task UseMessageMvc(this IServiceCollection _)
        {
            if (Interlocked.Increment(ref isInitialized) != 2)
            {
                return;
            }
            //捕获Ctrl+C事件
            Console.CancelKeyPress += ZeroFlowControl.OnShutdown;
            await ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
            Console.WriteLine("应用已启动.请键入 Ctrl+C 退出.");
            Process.GetCurrentProcess().WaitForExit();
        }
    }
}