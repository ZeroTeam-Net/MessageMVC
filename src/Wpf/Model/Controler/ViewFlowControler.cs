using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 模拟视图流程控制
    /// </summary>
    [Service("ViewFlow")]
    public class ViewFlowControler : IApiControler
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ViewFlowControler));

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="argument">参数</param>
        [Route("v1/step1")]
        public IApiResult<string> Step1()
        {
            Logger.Information("接收到步骤一的操作请求,正在驱动进入步骤二");
            return ApiResultHelper.Succees("step2");
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="argument">参数</param>
        [Route("v1/step2")]
        public IApiResult<string> Step2()
        {
            Logger.Information("接收到步骤二的操作请求,正在驱动进入步骤三");
            return ApiResultHelper.Succees("step3");
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="argument">参数</param>
        [Route("v1/step3")]
        public IApiResult<string> Step3()
        {
            Logger.Information("接收到步骤3的操作请求,完成流程");
            return ApiResultHelper.Succees("home");
        }
    }
}
