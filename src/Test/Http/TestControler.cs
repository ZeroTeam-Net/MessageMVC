using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {
        [Route("v1/plan")]
        public ApiResult Paln()
        {
            MessageProducer.Plan(new PlanTasks.PlanOption
            {
                plan_type = PlanTasks.plan_date_type.second,
                plan_repet = -1,
                plan_value = 10,
            }, "api", "v1/end", null);

            return ApiResult.Succees("Paln");
        }

        [Route("v1/do")]
        public ApiResult DoPlan()
        {
            return ApiResult.Succees("DoPlan");
        }
    }
}
