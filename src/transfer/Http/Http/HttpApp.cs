using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

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
        public static void AddMessageMvcHttpClient(this IServiceCollection services)
        {
            HttpClientOption.Instance.DefaultUrl ??= "";
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient<IMessagePoster, HttpPoster>();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcHttp(this IServiceCollection services)
        {
            HttpClientOption.Instance.DefaultUrl ??= "";
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient<IMessagePoster, HttpPoster>();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void UseMessageMVC(this IApplicationBuilder app)
        {
            app.Run(Call);
            app.ApplicationServices = DependencyHelper.RootProvider;
        }

        /// <summary>
        ///     配置使用MessageMVC
        /// </summary>
        /// <param name="host">主机生成器</param>
        /// <param name="registAction">配置注册方法</param>
        /// <param name="autoDiscove">是否自动发现API方法</param>
        public static IWebHostBuilder UseMessageMVC(this IWebHostBuilder host, Action<IServiceCollection> registAction, bool autoDiscove = true)
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
                    DependencyHelper.Flush();
                });
            return host;
        }


        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static async Task Call(HttpContext context)
        {
            if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                //HttpProtocol.CrosOption(context.Response);
                return;
            }
            if (context.Request.Path == "/")
            {
                await context.Response.WriteAsync("Wecome MessageMVC,Lucky every day!", Encoding.UTF8);
                return;
            }
            if (!ZeroAppOption.Instance.IsRuning)
            {
                await context.Response.WriteAsync(ApiResultHelper.PauseJson, Encoding.UTF8);
                return;
            }
            //HttpProtocol.CrosCall(context.Response);
            try
            {
                //命令
                var reader = new HttpMessageReader();
                var (success, message) = await reader.CheckRequest(context);
                //开始调用
                if (success)
                {
                    var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                    {
                        ServiceName = message.Service,
                        Receiver = new EmptyReceiver(),
                        Serialize = DependencyHelper.GetService<ISerializeProxy>()
                    };
                    await MessageProcessor.OnMessagePush(service, message, false, new HttpWriter
                    {
                        Context = context
                    });
                }
                else
                {
                    await context.Response.WriteAsync(ApiResultHelper.NotSupportJson, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                try
                {
                    await context.Response.WriteAsync(ApiResultHelper.BusinessErrorJson, Encoding.UTF8);
                }
                catch (Exception exception)
                {
                    ScopeRuner.ScopeLogger.Exception(exception);
                }
            }
        }

    }
}