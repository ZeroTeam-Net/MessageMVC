using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Consumer("PlanTask")]
    public class TestControler : IApiControler
    {
        [Route("v1/post")]
        public ApiResult Post(PlanTasks.PlanCallInfo info)
        {
            var item = new PlanTasks.PlanItem
            {
                Option = info.Option,
                Message = info.Message
            };
            item.CheckNextTime();
            return ApiResult.Succees();
        }
    }
}
