using Newtonsoft.Json;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("test")]
    public class ParallelTest : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("do"), ApiOption(ApiOption.CustomContent)]
        public async Task<string> ParallelCall()
        {
            var res = await MessagePoster.Post(new InlineMessage
            {
                Topic = "ParallelTest",
                Title= "hello"
            });
            return JsonConvert.SerializeObject(res.ResultData, Formatting.Indented);
        }
    }

    [Service("HttpTest1")]
    public class HttpTest1Controler : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("hello"),ApiOption(ApiOption.CustomContent)]
        public string Hello()
        {
            return "hello1";
        }
    }


}
