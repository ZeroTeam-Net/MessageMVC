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
        public void Text(IInlineMessage message)
        {
            Logger.Information(message.Argument);
        }
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("int")]
        public async Task<int> Text()
        {
            await Task.Delay(10);
            return DateTime.Now.ToTimestamp();
        }
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("result/base")]
        public async Task<IApiResult> Result()
        {
            await Task.Delay(10);
            return ApiResultHelper.Succees();
        }
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("result/data")]
        public async Task<IApiResult<int>> Result2()
        {
            await Task.Delay(10);
            return ApiResultHelper.Succees(DateTime.Now.ToTimestamp());
        }
    }
}
