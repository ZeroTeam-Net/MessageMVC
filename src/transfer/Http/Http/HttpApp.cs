using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public static void UseHttp(this IServiceCollection services)
        {
            services.AddTransient<IMessagePoster, HttpPoster>();
            services.AddTransient<IServiceReceiver, HttpReceiver>();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static async void RunMessageMVC(this IApplicationBuilder app, bool handerHttp)
        {
            if (handerHttp)
                app.Run(HttpReceiver.Call);

            DependencyHelper.LoggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            DependencyHelper.Update(app.ApplicationServices);
            await ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }
    }
}