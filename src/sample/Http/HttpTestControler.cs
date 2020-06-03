using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
//using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("HttpTest")]
    public class HttpTestControler : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("hello"),ApiOption(ApiOption.CustomContent)]
        public string Hello(string signature, string timestamp, string nonce, string echostr)
        {
            var msg = GlobalContext.CurrentNoLazy?.Message;
            if (msg == null || string.IsNullOrWhiteSpace(msg.Content))
            {
                return "hello";
            }
            var seri = new CDataXmlSerializeProxy();
            var dict = seri.ToObject<Dictionary<string, string>>(msg.Content);
            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("test")]
        public IApiResult Test(Argument arg)
        {
            return ApiResultHelper.Succees();
        }
    }
}
