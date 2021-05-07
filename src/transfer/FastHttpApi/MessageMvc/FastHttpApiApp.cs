using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// Http应用
    /// </summary>
    public static class FastHttpApiApp
    {
        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcFastHttpApi(this IServiceCollection services)
        {
            services.AddSingleton<IZeroOption>(pri => HttpMessageOption.Instance);
            services.AddTransient<IFlowMiddleware>(pr=>FastHttpApiLifeFlow.Instance);
            services.AddTransient(pri => GlobalContext.Current.Message is HttpMessage message ? message.HttpContext : null);
        }
    }
}