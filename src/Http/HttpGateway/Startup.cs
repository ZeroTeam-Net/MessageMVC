using System.IO;
using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    /// 启动基类
    /// </summary>
    public class Startup
    {

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 配置对象
        /// </summary>
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            IocHelper.SetServiceCollection(services);
            services.UseCsRedis();
            //services.AddSingleton<IFlowMiddleware, KafkaProducer>();//Kafka环境
            //services.AddSingleton<IMessageProducer, KafkaProducer>();//采用Kafka生产端
            MessageRoute.Initialize(services);
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.Run(MessageRoute.Call); 
        }
    }

}