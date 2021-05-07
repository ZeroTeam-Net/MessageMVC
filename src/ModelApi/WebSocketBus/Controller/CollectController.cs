using System.Linq;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.Zeroteam.WebSocketBus
{
    [Service("collect")]
    [Route("v1/rooms")]
    public class CollectController : IApiController
    {
        [Route("all")]
        public IApiResult<string[]> Rooms()
        {
            return ApiResultHelper.Succees(MessageRoomManage.Instance.Rooms.Keys.ToArray());
        }
        [Route("types")]
        public IApiResult<string[]> RoomInfo(string roomName)
        {
            var room = MessageRoomManage.Instance[roomName];
            return ApiResultHelper.Succees(room.messages.Keys.ToArray());
        }
        [Route("data")]
        public IApiResult<string> RoomData(string roomName, string type)
        {
            var room = MessageRoomManage.Instance[roomName];
            return ApiResultHelper.Succees(room?[type]?.Content);
        }
    }
}
