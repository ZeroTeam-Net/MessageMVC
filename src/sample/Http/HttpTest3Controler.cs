using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("test3")]
    public class HttpTest3Controler : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("arg")]
        public IApiResult<string> Hello(string name, int count, Guid guid,byte[] file, long id = -1, Em em = Em.Test)
        {
            return ApiResultHelper.Helper.Succees($"name:{name} count:{count} guid:{guid} file:{file} id:{id} em:{em}");
        }
    }
    public enum Em
    {
        None,
        Test
    }
}
