using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// Http应用
    /// </summary>
    public static class HttpApp
    {
        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcHttp(this IServiceCollection services)
        {
            HttpClientOption.Instance.DefaultUrl ??= "";
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient<IMessagePoster, HttpPoster>();
            services.AddTransient<IServiceReceiver, HttpReceiver>();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void UseMessageMVC(this IApplicationBuilder app)
        {
            app.Run(HttpReceiver.Call);
            app.ApplicationServices = DependencyHelper.RootProvider;
        }

        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="host">主机生成器</param>
        /// <param name="registAction">配置注册方法</param>
        /// <param name="autoDiscove">是否自动发现API方法</param>
        public static IWebHostBuilder UseMessageMVC(this IWebHostBuilder host, Action<IServiceCollection> registAction, bool autoDiscove=true)
        {
            host.ConfigureAppConfiguration((ctx, builder) =>
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
                    registAction(services);
                    ZeroApp.AddDependency(services, false);
                });
            return host;
        }
    }
}