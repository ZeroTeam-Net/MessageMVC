using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ZeroTeam.MessageMVC.Tcp.Sample
{

    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseMessageMVC(services => services.AddMessageMvcTcp())
                .ConfigureServices(services => services.AddHostedService<TestHost>());
        }
    }
}