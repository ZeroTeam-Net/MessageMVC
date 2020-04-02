using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Consumer("PlanTask")]
    internal class PlanTaskControler : IApiControler
    {
        [Route("v1/post")]
        public async Task<ApiResult> Post(PlanTasks.PlanCallInfo info)
        {
            var item = new PlanTasks.PlanItem
            {
                Option = info.Option,
                Message = info.Message
            };
            if (!await item.FirstSave())
            {
                return ApiResult.ArgumentError;
            }

            await item.CheckNextTime();
            return ApiResult.Succees();
        }
    }
}
