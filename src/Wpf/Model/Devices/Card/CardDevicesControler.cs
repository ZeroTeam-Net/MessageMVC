using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 模拟设备驱动命令
    /// </summary>
    [Service("CardDevices")]
    public class CardDevicesControler : IApiControler
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(CardDevicesControler));

        /// <summary>
        /// 处理卡片弹出命令
        /// </summary>
        [Route("v1/cmd/pull")]
        public async Task<IApiResult> CardPull(Argument argument)
        {
            Logger.Information("接收卡片弹出请求,正在驱动设备进行操作");
            await Task.Delay(100); /*驱动操作*/
            Logger.Information("驱动设备操作成功,正在发送设备事件");
            var state = await MessagePoster.PublishAsync("Devices", "v1/card/pull", argument);
            Logger.Information("卡片弹出请求操作完成");
            return ApiResultHelper.Helper.State(state.ToErrorCode());
        }
    }
}
