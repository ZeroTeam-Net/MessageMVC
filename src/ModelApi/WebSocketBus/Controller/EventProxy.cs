using Agebull.Common.Ioc;
using BeetleX.FastHttpApi;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.Zeroteam.WebSocketBus
{
    /// <summary>
    ///  事件代理
    /// </summary>
    public partial class EventProxy : IFlowMiddleware
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(EventProxy);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        Task IZeroDiscover.Discovery()
        {
            WebSocketOption.Load();
            var option = WebSocketOption.Instance;
            if(!ZeroAppOption.Instance.Services.Services.TryGetValue(option.ServiceName,out var receiver))
            {
                return Task.CompletedTask;
            }

            var service = new ZeroService
            {
                IsAutoService = true,
                ServiceName = option.ServiceName,
                Receiver = receiver.Receiver(),
                Serialize = new NewtonJsonSerializeProxy()
            } as IService;


            service.RegistWildcardAction(new ApiActionInfo
            {
                Name = "*",
                Routes = new[] { "*" },
                ResultType = typeof(void),
                ControllerName = option.ServiceName,
                AccessOption = ApiOption.Public | ApiOption.Anymouse,
                Action = (msg, seri, arg) => OnEvent(msg, seri, arg)
            });
            ZeroFlowControl.RegistService(service);
            FastHttpApiLifeFlow.HttpDisconnectHandlers.Add(OnClose);
            return Task.CompletedTask;
        }

        static void OnClose(ISession session)
        {
            MessageRoomManage.Instance.Exit(session);
        }

        static object OnEvent(IInlineMessage message, ISerializeProxy __, object _)
        {
            var roomName = message.Method.Split('/')[0];
            var group = MessageRoomManage.Instance[roomName];
            if (group == null)
            {
                MessageRoomManage.Instance.Rooms.TryAdd(roomName, group = new MessageRoom
                {
                    Name = roomName
                });
            }
            group.Push(message);
            return null;
        }
    }
}
