using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("Markpoint2")]
    public class RpcControler : IApiControler
    {
        [Route("test")]
        public IApiResult Result(Argument argument)
        {
            LogRecorder.Trace(GetType().FullName);
            return ApiResultHelper.Succees();
        }
    }
}
