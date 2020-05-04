using ZeroTeam.MessageMVC.ZeroApis;
using System.Threading.Tasks;
using System;
//using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using Agebull.Common.Ioc;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("PlanTest")]
    public class PlanControler : IApiController
    {
        ///// <summary>
        ///// 测试接口
        ///// </summary>
        ///// <param name="argument">参数</param>
        ///// <returns>ApiResult格式的成功或失败</returns>
        //[Route("v1/plan")]
        //public async Task<IOperatorStatus> Message()
        //{
        //    return  await PlanPoster.PostAsync(new PlanOption
        //    {
        //        PlanType = PlanTimeType.second,
        //        PlanValue = 1
        //    }, "PlanTest", "v1/end", new Argument
        //    {
        //        Value = DateTime.Now.ToString()
        //    });
        //}

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        [Route("v1/end")]
        public IApiResult Argument(Argument argument)
        {
            GlobalContext.Current.Status.LastMessage = "ddd";
            var ctx = DependencyHelper.Create<IZeroContext>();
            if (GlobalContext.Current.Status.LastMessage != ctx.Status.LastMessage)
                return ApiResultHelper.State(1);
            return ApiResultHelper.Succees(argument.Value);
        }
    }
}
