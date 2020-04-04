using Agebull.Common.Logging;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("test1")]
    public class TestControler : IApiControler
    {
        [Route("test/res")]
        public ApiResult Result()
        {
            LogRecorder.Trace("Result");
            return ApiResultHelper.Succees();
        }

        [Route("test/full")]
        public string Full(int name)
        {
            LogRecorder.Trace("Full");
            return "Full";
        }

        [Route("test/void")]
        public void GetVoid()
        {
            LogRecorder.Trace("GetVoid");
        }

        [Route("test/arg")]
        public void Argument(Argument arg)
        {
            LogRecorder.Trace($"Argument : {arg.Value}");
        }

        [Route("async/res")]
        public async Task<ApiResult> ResultAsync()
        {
            LogRecorder.Trace($"ResultAsync");
           return await Task.Factory.StartNew(TaskTest);
        }

        [Route("async/arg")]
        public async Task ArgumentAsync(Argument arg)
        {
            LogRecorder.Trace($"ArgumentAsync : {arg.Value}");
            await Task.Factory.StartNew(TaskTest);
        }

        [Route("async/full")]
        public Task<ApiResult> FullAsync(Argument arg)
        {
            LogRecorder.Trace($"FullAsync : {arg.Value}");
            LogRecorder.Trace(Task.CurrentId?.ToString());
            return Task.Factory.StartNew(TaskTest);
        }

        [Route("async/void")]
        public  Task VoidAsync()
        {
            LogRecorder.Trace(Task.CurrentId?.ToString());
            return  Task.Factory.StartNew(TaskTest);
        }


        public ApiResult TaskTest()
        {
            LogRecorder.Trace(Task.CurrentId?.ToString());
            Task.Delay(100);
            LogRecorder.Trace(Task.CurrentId?.ToString());
            LogRecorder.Trace("VoidAsync");
            return ApiResultHelper.Succees();
        }
    }
}
