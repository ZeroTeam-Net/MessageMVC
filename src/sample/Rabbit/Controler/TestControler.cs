using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.RabbitMQ;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [RabbitMQConsumer("test1")]
    public class TestControler : IApiController
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        public ILogger Logger { get; set; }


        [Route("test/res")]
        public IApiResult Result()
        {
            Logger.Trace("Result");
            return ApiResultHelper.Succees();
        }

        [Route("test/full")]
        public string Full(string name)
        {
            Logger.Trace("Full");
            return "Full";
        }

        [Route("test/void")]
        public void GetVoid()
        {
            Logger.Trace("GetVoid");
        }

        [Route("test/arg")]
        public void Argument(Argument arg)
        {
            Logger.Trace($"Argument : {arg.Value}");
        }

        [Route("async/res")]
        public async Task<IApiResult> ResultAsync()
        {
            Logger.Trace($"ResultAsync");
            return await Task.Factory.StartNew(TaskTest);
        }

        [Route("async/arg")]
        public async Task ArgumentAsync(Argument arg)
        {
            Logger.Trace($"ArgumentAsync : {arg.Value}");
            await Task.Factory.StartNew(TaskTest);
        }

        [Route("async/full")]
        public Task<IApiResult> FullAsync(Argument arg)
        {
            Logger.Trace($"FullAsync : {arg.Value}");
            Logger.Trace(Task.CurrentId?.ToString());
            return Task.Factory.StartNew(TaskTest);
        }

        [Route("async/void")]
        public Task VoidAsync()
        {
            Logger.Trace(Task.CurrentId?.ToString());
            return Task.Factory.StartNew(TaskTest);
        }


        public IApiResult TaskTest()
        {
            Logger.Trace(Task.CurrentId?.ToString());
            Task.Delay(100);
            Logger.Trace(Task.CurrentId?.ToString());
            Logger.Trace("VoidAsync");
            return ApiResultHelper.Succees();
        }
    }
}
