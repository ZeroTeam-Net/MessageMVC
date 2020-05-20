using Agebull.Common.Ioc;
//using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("HttpTest")]
    public class PlanControler : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("hello")]
        public string Hello()
        {
            return "hello";
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("test")]
        public IApiResult Test()
        {
            return ApiResultHelper.Succees();
        }
    }
}
