using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC.PlanTasks;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {
        [Route("v1/plan")]
        public async Task<ApiResult> Paln()
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
        public ApiResult DoPlan()
        {
            return ApiResultHelper.Succees("DoPlan");
        }
    }
}
