using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("test2")]
    public class HttpTest3Controler : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("hello"), ApiOption(ApiOption.CustomContent)]
        public IApiResult<string> Hello(string abc)
        {
            return ApiResultHelper.Helper.Succees($"hello1:{abc}");
        }
    }
}
