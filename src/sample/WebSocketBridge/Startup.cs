using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Web;

namespace WebNotifyTest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.BindingMessageMvc();
            services.AddMessageMvcRedis();
            services.AddMessageMvc(false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseDefaultFiles("/index.htm");
            app.UseWebSocketNotify();
            app.UseMessageMVC(false);
        }
    }
}
