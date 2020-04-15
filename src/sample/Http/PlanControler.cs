using ZeroTeam.MessageMVC.ZeroApis;
using System.Threading.Tasks;
using System;
using ZeroTeam.MessageMVC.PlanTasks;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("PlanTest")]
    public class PlanControler : IApiControler
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/plan")]
        public async Task<IOperatorStatus> Message()
        {
            return  await PlanPoster.PostAsync(new PlanOption
            {
                plan_type = plan_date_type.second,
                plan_value = 10
            }, "PlanTest", "v1/end", new Argument
            {
                Value = DateTime.Now.ToString()
            });
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        [Route("v1/end")]
        public IApiResult Argument(Argument argument)
        {
            return ApiResultHelper.Succees(argument.Value);
        }
    }
}
