using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
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
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="builder">主机生成器</param>
        /// <param name="registAction">配置注册方法</param>
        public static IHostBuilder UseMessageMVC(this IHostBuilder builder, Action<IServiceCollection> registAction)
        {
            UseMessageMVC(builder, registAction, true, null);
            return builder;
        }

        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="builder">主机生成器</param>
        /// <param name="autoDiscove">是否自动发现API方法</param>
        /// <param name="registAction">配置注册方法</param>
        public static IHostBuilder UseMessageMVC(this IHostBuilder builder, bool autoDiscove, Action<IServiceCollection> registAction)
        {
            UseMessageMVC(builder, registAction, autoDiscove, null);
            return builder;
        }

        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="builder">主机生成器</param>
        /// <param name="registAction">配置注册方法</param>
        /// <param name="discovery">自定义API发现方法</param>
        public static IHostBuilder UseMessageMVC(this IHostBuilder builder, Action<IServiceCollection> registAction, Action discovery)
        {
            UseMessageMVC(builder, registAction, false, discovery);
            return builder;
        }

        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="builder">主机生成器</param>
        /// <param name="registAction">配置注册方法</param>
        /// <param name="autoDiscovery">自动发现</param>
        /// <param name="discovery">自定义API发现方法</param>
        internal static void UseMessageMVC(this IHostBuilder builder, Action<IServiceCollection> registAction, bool autoDiscovery, Action discovery)
        {
            Console.Write(@"-------------------------------------------------------------
---------------> ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(@"Wecome ZeroTeam MessageMVC");
            Console.ResetColor();
            Console.WriteLine(@" <----------------
-------------------------------------------------------------");
            builder.ConfigureAppConfiguration((ctx, builder) =>
            {
                DependencyHelper.ServiceCollection.AddSingleton(p => builder);
                ConfigurationHelper.BindBuilder(builder);
                ZeroFlowControl.LoadConfig();
                ZeroAppOption.Instance.AutoDiscover = autoDiscovery;
                ZeroAppOption.Instance.Discovery = discovery;
                ConfigurationHelper.OnConfigurationUpdate = cfg => ctx.Configuration = cfg;
                ctx.Configuration = ConfigurationHelper.Root;
            })
            .ConfigureServices((ctx, services) =>
            {
                DependencyHelper.Binding(services);
                services.AddHostedService<ZeroHostedService>();
                registAction(services);
                AddDependency(services);
            });
        }
        /// <summary>
        /// 检查并注入配置
        /// </summary>
        internal static void AddDependency(IServiceCollection services)
        {
            ZeroFlowControl.LoadAddIn();
            //IZeroContext构造
            services.AddSingleton<IZeroOption>(pri=> MessagePostOption.Instance);
            services.TryAddTransient<IUser, UserInfo>();
            services.TryAddTransient<IZeroContext, ZeroContext>();
            //消息发送
            services.AddTransient<IFlowMiddleware, MessagePoster>();
            //API路由与执行
            services.AddTransient<IMessageMiddleware, ApiExecuter>();
            //消息格式
            services.AddTransient<IInlineMessage, InlineMessage>();
            services.AddTransient<IMessageResult, MessageResult>();

            //序列化工具
            if (ZeroAppOption.Instance.UsMsJson)
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
            DependencyHelper.Flush();
        }

        static int isInitialized;
        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，适用于测试的场景
        /// </summary>
        /// <param name="services">依赖服务</param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static async Task UseTest(this IServiceCollection services, Assembly assembly = null)
        {
            if (Interlocked.Increment(ref isInitialized) != 1)
            {
                return;
            }
            services.TryAddTransient<IServiceReceiver, EmptyReceiver>();
            services.TryAddTransient<IMessageConsumer, EmptyReceiver>();
            services.TryAddTransient<INetEvent, EmptyReceiver>();
            AddDependency(services);
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