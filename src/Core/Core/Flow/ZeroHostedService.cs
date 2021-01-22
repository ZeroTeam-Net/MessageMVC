using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 主机服务
    /// </summary>
    public class ZeroHostedService : IHostedService
    {
        #region 主机流程

        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        CancellationTokenSource _tokenSource;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="appLifetime"></param>
        public ZeroHostedService(ILogger<ZeroHostedService> logger, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _tokenSource = new CancellationTokenSource();
            _appLifetime.ApplicationStarted.Register(()=> ZeroFlowControl.OnStarted(_tokenSource));
            _appLifetime.ApplicationStopping.Register(()=> ZeroFlowControl.OnStopping(_tokenSource));
            ZeroFlowControl._logger = _logger;

            await ZeroFlowControl.Check();
            if (ZeroAppOption.Instance.AutoDiscover)
                ZeroFlowControl.Discove();

            await ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return ZeroFlowControl.Shutdown();
        }
        #endregion

    }
}