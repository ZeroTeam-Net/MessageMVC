using System;
using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroApis;
using ZeroTeam.MessageMVC.ZeroApis;

namespace WebApplication2.Controllers
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
