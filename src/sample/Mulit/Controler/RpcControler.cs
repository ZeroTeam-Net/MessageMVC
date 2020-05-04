using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("Markpoint2")]
    public class RpcControler : IApiController
    {
        [Route("test")]
        public IApiResult Result()
        {
            LogRecorder.Trace(GetType().FullName);
            return ApiResultHelper.Succees();
        }
    }
}
