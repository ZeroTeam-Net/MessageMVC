using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC.PlanTasks;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.Context;
using Agebull.Common.Ioc;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        ILogger logger;

        /// <summary>
        /// 当前登录用户,自动依赖构造后设置值
        /// </summary>
        [FromServices]
        public IUser User { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="log">日志对象,框架自动构造</param>
        public TestControler([FromServices]ILogger<TestControler> log)
        {
            logger = log;
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/call")]
        public async Task<IApiResult> Call(Argument argument)
        {
            logger.LogDebug($"Call({argument.Value})");
            await Task.Delay(100);
            return ApiResultHelper.Succees();
        }

        [Route("v1/plan")]
        public async Task<IApiResult> Paln()
        {
            await PlanPoster.PostAsync(new PlanTasks.PlanOption
            {
                plan_type = plan_date_type.second,
                plan_repet = 5,
                retry_set = -1,
                plan_value = 10,
            }, "api", "v1/do", null);

            return ApiResultHelper.Succees("Paln");
        }

        [Route("v1/do")]
        public IApiResult DoPlan()
        {
            return ApiResultHelper.Succees("DoPlan");
        }
    }
}
