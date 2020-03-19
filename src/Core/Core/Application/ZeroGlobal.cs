using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 默认的全局对象
    /// </summary>
    public class ZeroGlobal : IAppMiddleware
    {
        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public void CheckOption(ZeroAppConfigRuntime config)
        {
            var testContext = IocHelper.Create<GlobalContext>();
            if (testContext == null)
                IocHelper.AddScoped<GlobalContext, GlobalContext>();
            GlobalContext.ServiceName = config.ServiceName;
            GlobalContext.ServiceRealName = $"{config.ServiceName}:{config.StationName}:{RandomOperate.Generate(4)}";

            //日志
            LogRecorder.LogPath = config.LogFolder;
            LogRecorder.GetMachineNameFunc = () => GlobalContext.ServiceRealName;
            LogRecorder.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.UserId.ToString() ?? "*";
            LogRecorder.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Request?.RequestId ?? RandomOperate.Generate(10);
            LogRecorder.Initialize();
            IocScope.Logger = IocHelper.Create<ILoggerFactory>().CreateLogger("MicroZero");
        }
    }
}
