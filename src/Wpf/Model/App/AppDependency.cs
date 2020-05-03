using Agebull.EntityModel.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.ConfigSync;
using ZeroTeam.MessageMVC.Messages;

namespace MessageMVC.Wpf.Sample
{
    public static class AppDependency
    {
        /// <summary>
        /// 应用入口
        /// </summary>
        /// <param name="services"></param>
        public static void UseSampleApp(this IServiceCollection services)
        {
            services.AddTransient<MainDataModel>();
            services.AddTransient<IServiceReceiver, EmptyReceiver>();
            services.AddTransient<INetEvent, EmptyReceiver>();
            services.UseFlow(typeof(DevicesEventControler).Assembly, false);

            //Task.Factory.StartNew(Simulation);
        }

        private static async void Simulation()
        {
            await Task.Delay(1000);

            var random = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
            while (ZeroFlowControl.IsAlive)
            {
                var time = random.Next(1000, 10000);
                if ((time % 2) == 1)
                {
                    await MessagePoster.PublishAsync("Devices", "v1/card/push", new Argument<int>
                    {
                        Value = time
                    });
                }
                else
                {
                    await MessagePoster.PublishAsync("Devices", "v1/card/pull", new Argument<int>
                    {
                        Value = time
                    });
                }
                await Task.Delay(time);
            }
        }
    }
}