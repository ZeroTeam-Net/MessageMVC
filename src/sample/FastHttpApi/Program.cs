using BeetleX.FastHttpApi;
using Microsoft.Extensions.Hosting;

namespace ZeroTeam.MessageMVC.Tcp.Sample
{

    public class Program
    {

        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseMessageMVC(services => services.AddMessageMvcFastHttpApi())
                .Build()
                .Run();
        }

    }
}