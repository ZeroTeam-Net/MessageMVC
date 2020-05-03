using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using MessageMVC.Wpf.Sample.Model;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 模拟设备驱动事件
    /// </summary>
    [NetEvent("Devices")]
    public class DevicesEventControler : IApiControler
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(DevicesEventControler));

        /// <summary>
        /// 接收到卡插入事件
        /// </summary>
        [Route("v1/card/push"), ApiAccessOptionFilter(ApiAccessOption.ArgumentCanNil)]
        public IApiResult CardPush(Argument argument)
        {
            Logger.Information("接收到卡插入事件,正在驱动业务流程");
            BusinessFlowControl.Instance.CardPush(argument?.Value);
            Logger.Information("卡插入事件处理完成");
            return ApiResultHelper.Succees();
        }

        /// <summary>
        /// 接收到卡拨出事件
        /// </summary>
        [Route("v1/card/pull")]
        public IApiResult CardPull(Argument argument)
        {
            Logger.Information("接收接收到卡拨出事件,正在驱动业务流程");
            BusinessFlowControl.Instance.CardPull(argument?.Value);
            Logger.Information("卡拨出事件处理完成");
            return ApiResultHelper.Succees();
        }
    }
}
