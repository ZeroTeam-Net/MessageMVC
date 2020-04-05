using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Inproc("Inproc")]
    public class InprocControler : IApiControler
    {
        [Route("test")]
        public IApiResult Result()
        {
            LogRecorder.Trace(GetType().FullName);
            return ApiResultHelper.Succees();
        }
    }
}
