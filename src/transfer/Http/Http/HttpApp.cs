using Microsoft.AspNetCore.Builder;
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
        public static void UseHttp(this IServiceCollection services)
        {

            services.AddTransient<IMessagePoster, HttpPoster>();

            services.AddTransient<IServiceReceiver, HttpReceiver>();

            services.UseFlowByAutoDiscover();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void UseHttp(this IServiceCollection services, Type type)
        {
            services.AddTransient<IMessagePoster, HttpPoster>();

            services.AddTransient<IServiceReceiver, HttpReceiver>();

            services.UseFlow(type.Assembly, false);
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void RunMessageMVC(this IApplicationBuilder app)
        {
            app.Run(HttpRoute.Call);
        }
    }
}