using System.ComponentModel;
using System.Threading.Tasks;
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
        public string Hello(string abc)
        {
            return "hello1";
        }
        
        /// <summary>
         /// 获取设备标识
         /// </summary>
         /// <returns></returns>
        [Route("v1/did/refresh"), Category("令牌")]
        [ApiOption(ApiOption.Public | ApiOption.Anymouse)]
        public async Task<IApiResult<string>> GetDeviceToken(string did)
        {
            return ApiResultHelper.Succees("hello1");
        }
    }
}
