using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="builder">主机生成器</param>
        /// <param name="action">配置注册方法</param>
        public static IHostBuilder UseMessageMVC(this IHostBuilder builder, Action<IServiceCollection> action)
        {
            return UseMessageMVC(builder, true, action);
        }

        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="builder">主机生成器</param>
        /// <param name="autoDiscove">是否自动发现API方法</param>
        /// <param name="action">配置注册方法</param>
        public static IHostBuilder UseMessageMVC(this IHostBuilder builder, bool autoDiscove, Action<IServiceCollection> action)
        {
            builder.ConfigureAppConfiguration((ctx, builder) =>
                {
                    DependencyHelper.ServiceCollection.AddSingleton(p => builder);
                    ConfigurationHelper.BindBuilder(builder);
                    ConfigurationHelper.Flush();
                    ctx.Configuration = ConfigurationHelper.Root;
                    ZeroAppOption.LoadConfig();
                    ZeroAppOption.Instance.AutoDiscover = autoDiscove;
                })
                .ConfigureServices((ctx, services) =>
                {
                    DependencyHelper.Binding(services);
                    services.AddHostedService<ZeroHostedService>();
                    action(services);
                    AddDependency(services, false);
                });
            return builder;
        }

        /// <summary>
        /// 检查并注入配置
        /// </summary>
        public static void AddDependency(IServiceCollection services, bool msJson)
        {
            //IZeroContext构造
            services.TryAddTransient<IZeroContext, ZeroContext>();
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

            //序列化工具
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

        static int isInitialized;
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
            //ZeroFlowControl.Check();
            //ZeroFlowControl.Discove(assembly);
            //ZeroFlowControl.Check();
            //if (assembly != null)
            //    ZeroFlowControl.Discove(assembly);
            await ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

    }
}