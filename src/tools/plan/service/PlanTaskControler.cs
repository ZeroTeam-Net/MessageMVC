using Agebull.Common.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    [Consumer("PlanTask")]
    internal class PlanTaskControler : IApiControler
    {
        [Route("v1/post")]
        public async Task<IApiResult> Post(PlanCallInfo info)
        {
            var item = new PlanItem
            {
                Option = info.Option,
                Message = info.Message
            };

            if (!await item.FirstSave())
            {
                LogRecorder.Error($"校验不通过:{info.ToJson()}");
                return ApiResultHelper.State(OperatorStatusCode.ArgumentError);
            }

            await item.CheckNextTime();
            return ApiResultHelper.Succees();
        }
    }
}
