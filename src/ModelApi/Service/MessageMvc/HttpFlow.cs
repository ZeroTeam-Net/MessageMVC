using BeetleX.Zeroteam.WebSocketBus;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;

namespace BeetleX.FastHttpApi
{
    public class HttpFlow : IFlowMiddleware
    {
        public static HttpFlow Instance = new HttpFlow();
        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(HttpFlow);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        public MessageMvcHttpApiServer Server { get; private set; }

        Task IZeroDiscover.Discovery()
        {
            Server = new MessageMvcHttpApiServer();
            Server.Register(typeof(WebSocketController).Assembly);

            Server.HttpDisconnect += (o, e) =>
            {
                MessageRoomManage.Instance.Exit(e.Session);
            };
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Open()
        {
            Server.Open();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Closing()
        {
            Server.Dispose();
            return Task.CompletedTask;
        }
    }
}
