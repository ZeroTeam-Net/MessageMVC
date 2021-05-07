using Agebull.Common.Ioc;
using BeetleX.FastHttpApi;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.Zeroteam.WebSocketBus
{
    [Service("ws")]
    [Route("v1/room")]
    public class WebSocketController : IApiController
    {
        [Route("enter")]
        public IApiResult Enter(string token, [FromServices] IHttpContext context)
        {
            if(MessageRoomManage.Instance.Login(token, context))
                return ApiResultHelper.Succees();
            return ApiResultHelper.State(OperatorStatusCode.DenyAccess);
        }
        [Route("join")]
        public IApiResult Join(string room, [FromServices] IHttpContext context)
        {
            MessageRoomManage.Instance.Join(room, context);
            return ApiResultHelper.Succees();
        }

        [Route("left")]
        public IApiResult Left(string room, [FromServices] IHttpContext context)
        {
            MessageRoomManage.Instance.Left(room, context);
            return ApiResultHelper.Succees();
        }
    }
}
