using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("test1")]
    public class TestControler : ApiControllerBase
    {
        [Route("api/test")]
        public ApiResult Get()
        {
            LogRecorder.Trace("Call");
            return ApiResult.Succees();
        }
    }
}
