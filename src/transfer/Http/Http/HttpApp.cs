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
            services.AddSingleton<IZeroOption>(pri => HttpClientOption.Instance);
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient<IMessagePoster, HttpPoster>();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        [Obsolete]
        public static void AddMessageMvcHttp(this IServiceCollection services)
        {
            services.AddSingleton<IZeroOption>(pri => HttpClientOption.Instance);
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
        /// <param name="builder">主机生成器</param>
        /// <param name="registAction">配置注册方法</param>
        public static IWebHostBuilder UseMessageMVC(this IWebHostBuilder builder, Action<IServiceCollection> registAction)
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
        public static IWebHostBuilder UseMessageMVC(this IWebHostBuilder builder, bool autoDiscove, Action<IServiceCollection> registAction)
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
        public static IWebHostBuilder UseMessageMVC(this IWebHostBuilder builder, Action<IServiceCollection> registAction, Action discovery)
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
        internal static void UseMessageMVC(this IWebHostBuilder builder, Action<IServiceCollection> registAction, bool autoDiscovery, Action discovery)
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
                ZeroApp.AddDependency(services);
            });
        }

        #region AspNet截取

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

        #endregion
    }
}