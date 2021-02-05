using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Tcp.Sample
{
    [Service("tcp")]
    public class TcpControler : IApiController
    {
        /// <summary>
        /// 日志
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("test")]
        public void Text(IInlineMessage message)
        {
            Logger.Information(message.Argument);
        }
    }
}
