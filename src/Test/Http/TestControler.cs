using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {
        [Route("v1/test")]
        public async Task<ApiResult> OnOrderNew(Argument argument)
        {
            await Task.Yield();
            return ApiResult.Succees(argument?.Value);
        }
    }
}
