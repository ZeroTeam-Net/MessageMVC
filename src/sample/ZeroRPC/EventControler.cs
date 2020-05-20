using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [NetEvent("MarkPoint")]
    public class EventControler : IApiController
    {
        [Route("post"), SerializeType(SerializeType.NewtonJson)]
        public IApiResult Result(TraceLinkMessage argument)
        {
            LogRecorder.Trace(GetType().FullName);
            return ApiResultHelper.Succees();
        }
    }
}
