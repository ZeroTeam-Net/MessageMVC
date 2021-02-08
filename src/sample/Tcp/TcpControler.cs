using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
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
        public void Test(IInlineMessage message)
        {
            Logger.Information(message.Argument);
        }
    }
}
