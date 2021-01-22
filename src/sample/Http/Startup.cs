using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.RedisMQ;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// 启动类
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app,IWebHostEnvironment environment)
        {
            app.UseStaticFiles();

            //app.UseRouting();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
            
            app.UseMessageMVC();
        }
    }
}