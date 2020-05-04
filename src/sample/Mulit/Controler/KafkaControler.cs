using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("test1")]
    public class KafkaControler : IApiController
    {
        [Route("test")]
        public IApiResult Result()
        {
            LogRecorder.Trace(GetType().FullName);
            return ApiResultHelper.Succees();
        }
    }
}
